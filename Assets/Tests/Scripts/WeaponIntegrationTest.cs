using System.Collections;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class WeaponIntegrationTest
    {
        const string sceneName = "Assets/Tests/Scenes/GunTest";
        AssetBundle assetBundle;

        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Bundles", "weaponsystem-test-scenes.assetbundle");
            assetBundle = AssetBundle.LoadFromFile(path);
            Debug.Log("Loaded AssetBundle " + assetBundle);
            foreach (var scenes in assetBundle.GetAllScenePaths())
            {
                Debug.Log("Paths: " + scenes);
            }
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
        }

        [SetUp]
        public void SetUp()
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        [PrebuildSetup(typeof(PreBuildSetupTest))]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            var test = new MonoBehaviourTest<ValidateHitTest>();
            yield return test;

            var hitTest = test.component;
            hitTest.hitDetector.hit.Should().BeFalse("the projectiles are artificial projectiles and have no rigidbody.");
            hitTest.hitDetector.projectile.Should().NotBeNull();
            hitTest.hitDetector.source.Should().Be(hitTest.gun.gameObject);
        }
    }
}
