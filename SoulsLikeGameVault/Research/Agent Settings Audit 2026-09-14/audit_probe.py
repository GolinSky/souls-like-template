"""Read-only Codex config/skill probe; writes only sanitized evidence beside this script."""
import json, pathlib, subprocess, threading, queue, time, os, hashlib, tomllib, re, sys
BASE = pathlib.Path(__file__).parent
REPO = pathlib.Path(r"F:\Private\SoulsLikeTemplate")
HOME = pathlib.Path(r"C:\Users\golin")
def clean(value, key=""):
    if re.search(r"secret|password|token|authorization|credential|certificate|api.?key", key, re.I):
        return "<redacted present>"
    if isinstance(value, dict):
        if key in ("env","http_headers","headers","set"):
            return {k:"<redacted value>" for k in value}
        return {k:clean(v,k) for k,v in value.items()}
    if isinstance(value,list): return [clean(x,key) for x in value]
    if isinstance(value,str): return re.sub(r"(https?://[^\s\"'?#]+)\?[^\s\"'#]*",r"\1?<redacted-query>",value)
    return value
def write(name, data):
    (BASE/name).write_text(json.dumps(clean(data),indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
def probe():
    command = ["codex.cmd", "app-server", "--stdio"]
    if "--strict" in sys.argv: command.append("--strict-config")
    process = subprocess.Popen(command,cwd=REPO,stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,encoding="utf-8",creationflags=subprocess.CREATE_NO_WINDOW)
    out=queue.Queue(); err=[]
    def read():
        for line in process.stdout:
            try: out.put(json.loads(line))
            except ValueError: pass
    threading.Thread(target=read,daemon=True).start()
    def errors():
        for line in process.stderr: err.append(line)
    threading.Thread(target=errors,daemon=True).start()
    sequence=0
    def request(method,params):
        nonlocal sequence
        sequence+=1; ident=sequence
        process.stdin.write(json.dumps({"id":ident,"method":method,"params":params})+"\n");process.stdin.flush()
        until=time.monotonic()+25
        while time.monotonic()<until:
            try: msg=out.get(timeout=0.5)
            except queue.Empty:
                if process.poll() is not None: raise RuntimeError("app-server exited "+str(process.returncode))
                continue
            if msg.get("id")==ident:return msg
        raise TimeoutError(method)
    try:
        init=request("initialize",{"clientInfo":{"name":"agent_config_readonly_audit","version":"1.0.0"},"capabilities":{"experimentalApi":True}})
        write("client_initialize.json",init)
        process.stdin.write('{"method":"initialized","params":{}}\n');process.stdin.flush()
        for name,cwd in [("root",str(REPO)),("nested",str(REPO/"Assets/Scripts/Items")),("outside",str(HOME/"Documents"))]:
            response=request("config/read",{"cwd":cwd,"includeLayers":True})
            write("config_"+name+".sanitized.json",response)
            result=response.get("result",{}); config=result.get("config",{})
            wanted={k:config.get(k) for k in ["model","model_reasoning_effort","approval_policy","approvals_reviewer","sandbox_mode","project_doc_max_bytes","project_doc_fallback_filenames","agents","features"]}
            print(json.dumps({"scenario":name,"error":response.get("error"),"selected":wanted,"layers":[{k:v for k,v in x.items() if k!="config"} for x in result.get("layers",[])],"origins":{k:v for k,v in result.get("origins",{}).items() if k in wanted}},ensure_ascii=False))
        req=request("configRequirements/read",{})
        write("requirements.sanitized.json",req)
        print("requirements",json.dumps(clean(req)))
        skills=request("skills/list",{"cwds":[str(REPO),str(REPO/"Assets/Scripts/Items"),str(HOME/"Documents")],"forceReload":False})
        write("skills_discovery.json",skills)
        for item in skills.get("result",{}).get("data",[]):
            print(json.dumps({"skills_cwd":item.get("cwd"),"count":len(item.get("skills",[])),"errors":item.get("errors"),"selected":[{k:s.get(k) for k in ["name","path","scope","enabled"]} for s in item.get("skills",[]) if s.get("name") in ["graphify","unity-mcp-orchestrator","soulslike-context","unity-cli"]]}))
    except Exception as exc:
        print("probe_error",type(exc).__name__,str(exc))
    finally:
        process.terminate()
        try:process.wait(timeout=5)
        except subprocess.TimeoutExpired:process.kill();process.wait()
        write("client_stderr_strict.sanitized.json" if "--strict" in sys.argv else "client_stderr.sanitized.json",err)
        print("stderr_summary",json.dumps(clean(err[-8:])))
def manifest():
    paths=set()
    for s in ["AGENTS.md","GEMINI.md",".codex/config.toml",".agents/mcp_config.json",".agents/README.md"]:
        paths.add(REPO/s)
    for folder in [REPO/".codex/agents",REPO/".agents/skills",HOME/".agents/skills/graphify",HOME/".codex/skills/unity-mcp-skill"]:
        for root,dirs,files in os.walk(folder,followlinks=False):
            dirs[:]=[d for d in dirs if d not in [".git","node_modules","__pycache__"]]
            for name in files: paths.add(pathlib.Path(root)/name)
    for s in [".codex/config.toml",".codex/AGENTS.md",".codex/rules/default.rules",".claude/CLAUDE.md",".claude/settings.json",".gemini/GEMINI.md",".gemini/settings.json"]:
        paths.add(HOME/s)
    entries=[]
    for p in sorted(paths):
        if not p.is_file():continue
        b=p.read_bytes()
        entries.append({"path":str(p),"bytes":len(b),"sha256":hashlib.sha256(b).hexdigest(),"mtime_ns":p.stat().st_mtime_ns,"resolved":str(p.resolve()),"is_symlink":p.is_symlink(),"scope":"project" if p.is_relative_to(REPO) else "personal","content_review":"entry points and selected resources; manifest is presence/hash evidence"})
    write("inventory_manifest.json",entries)
    print("manifest_count",len(entries))
if __name__=="__main__":
    if "--manifest" in sys.argv:manifest()
    else:probe()
