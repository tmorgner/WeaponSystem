using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GunTest
    {
        const string prefabName = "assets/tests/models/testgun.prefab";
        AssetBundle assetBundle;

        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Bundles", "weaponsystem-test.assetbundle");
            assetBundle = AssetBundle.LoadFromFile(path);
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

        // Test that bundles can be loaded.
        [Test]
        public void GunTestSimplePasses()
        {
            // Use the Assert class to test conditions
            var assetNames = assetBundle.GetAllAssetNames();
            assetNames.Should().Contain(prefabName);
            foreach (var name in assetNames)
            {
                Debug.Log("Name: " + name);
            }

            var prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            prefab.Should().NotBeNull();
        }
    }
}
