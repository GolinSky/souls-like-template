"""Prepare proposals only. Never writes live configuration."""
from pathlib import Path
import difflib, json, tomllib, hashlib
BASE=Path(__file__).parent
REPO=Path(r"F:\Private\SoulsLikeTemplate")
HOME=Path(r"C:\Users\golin")
out=BASE/"patches";out.mkdir(exist_ok=True)
entries=[]
def proposal(filename,target,old,new,label):
    assert old!=new
    if target.suffix==".toml":tomllib.loads(new)
    if b"\r\n" in target.read_bytes():
        old = old.replace("\n", "\r\n")
        new = new.replace("\n", "\r\n")
    patch="".join(difflib.unified_diff(old.splitlines(True),new.splitlines(True),fromfile="a/"+label,tofile="b/"+label,n=2))
    (out/filename).write_text(patch,encoding="utf-8",newline="")
    entries.append({"patch":filename,"target":str(target),"baseline_sha256":hashlib.sha256(target.read_bytes()).hexdigest(),"status":"staged only","parser_check":"pass" if target.suffix==".toml" else "not applicable"})
def read(p):return p.read_text(encoding="utf-8-sig")
p=HOME/".codex/config.toml";s=read(p)
proposal("01-remove-unknown-rmcp-feature.patch",p,s,s.replace("rmcp_client = true\n",""),"config.toml")
p=REPO/".agents/skills/soulslike-probuilder/SKILL.md";s=read(p)
old="command schema before invoking an operation. Read\n[command-contract.md](references/command-contract.md) for the implemented surface."
new="command schema before invoking an operation. Use \x60unity command --json\x60 from\nthe verified project root to discover the implemented command surface and parameters."
assert old in s
proposal("02-repair-probuilder-command-reference.patch",p,s,s.replace(old,new),".agents/skills/soulslike-probuilder/SKILL.md")
p=REPO/".codex/config.toml";s=read(p)
addition='\n# Windows project scope: retain personal skills for other repositories.\n[[skills.config]]\npath = "C:/Users/golin/.agents/skills/graphify"\nenabled = false\n\n[[skills.config]]\npath = "C:/Users/golin/.codex/skills/unity-mcp-skill"\nenabled = false\n'
proposal("03-scope-competing-personal-skills.patch",p,s,s.rstrip()+"\n"+addition,".codex/config.toml")
addition='\n# This service targets the separate CV project.\n[mcp_servers.cv_filesystem]\nenabled = false\n'
proposal("04-exclude-cv-filesystem-from-soulslike.patch",p,s,s.rstrip()+"\n"+addition,".codex/config.toml")
p=REPO/".codex/agents/README.md";s=read(p)
old="- Only the MCP server required by the role is enabled. This reduces tool-selection noise and accidental cross-role work."
new="- Role files inherit unspecified MCP servers and connector capabilities; only explicit overrides narrow them. Inspect the spawned role's available tools before relying on isolation.\n- Role sandbox settings are defaults. Parent turn permission overrides can take precedence, so \x60sandbox_mode = \"read-only\"\x60 alone does not establish a read-only runtime."
assert old in s
proposal("05-correct-role-isolation-documentation.patch",p,s,s.replace(old,new),".codex/agents/README.md")
(out/"manifest.json").write_text(json.dumps(entries,indent=2)+"\n",encoding="utf-8")
print(json.dumps(entries,indent=2))
