using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityTools;

namespace Tests
{
    public class PhysicsToolsTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void RayShpereIntersectionInsideBeforeCenter()
        {
            var result = PhysicsTools.FindSphereIntersection(Vector3.zero,
                                                             3,
                                                             Vector3.forward,
                                                             Vector3.forward);
            result.Should().Be(2);
        }

        [Test]
        public void RayShpereIntersectionInsideBehindCenter()
        {
            var result2 = PhysicsTools.FindSphereIntersection(Vector3.zero,
                                                             3,
                                                             -Vector3.forward,
                                                             Vector3.forward);
            result2.Should().Be(4);
        }

        [Test]
        public void RayShpereIntersectionTangent()
        {
            var result = PhysicsTools.FindSphereIntersection(Vector3.zero,
                                                             3,
                                                             new Vector3(3, 0, -10),
                                                             Vector3.forward);
            result.Should().Be(10);
        }

        [Test]
        public void RayShpereIntersectionTangent2()
        {
            var result = PhysicsTools.FindSphereIntersection(Vector3.zero,
                                                             3,
                                                             new Vector3(0, 0, 3),
                                                             Vector3.forward);
            result.Should().Be(0);
        }


        [Test]
        public void RayShpereIntersectionNone()
        {
            var result = PhysicsTools.FindSphereIntersection(Vector3.zero,
                                                             3,
                                                             new Vector3(0, 0, 4),
                                                             Vector3.forward);
            result.Should().Be(-1);
        }

    }
}
