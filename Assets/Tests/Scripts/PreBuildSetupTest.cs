using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PreBuildSetupTest: IPrebuildSetup, IPostBuildCleanup
    {
        public void Setup()
        {
            Debug.Log("OnBuildSetup: " + Application.isEditor);
        }

        public void Cleanup()
        {
            
            Debug.Log("OnBuildCleanup: " + Application.isEditor);
        }
    }
}