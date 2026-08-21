#if HAS_RIGGING
using UnityEngine.Animations.Rigging;

namespace Jorjouto.AnimComposerSystem
{
    public class AcsRigBuilder : RigBuilder
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            onAddRigBuilder?.Invoke(this);
        }

        // Update is called once per frame
        void Update() {}
    }
}
#endif
