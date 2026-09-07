#if UNITY_EDITOR
using NUnit.Framework;
using SoulsLike.Entities.Elevator;

namespace SoulsLike.Editor.Tests.Elevator
{
    public sealed class ElevatorMotionTests
    {
        [Test]
        public void TriangularProfile_ReachesExactEndpoint()
        {
            ElevatorMotion motion = new(distance: 2f, maxSpeed: 10f, acceleration: 2f);

            Assert.That(motion.CruiseDuration, Is.EqualTo(0f));
            Assert.That(motion.SampleDistance(motion.AccelerationDuration), Is.EqualTo(1f));
            Assert.That(motion.SampleDistance(motion.Duration), Is.EqualTo(2f));
            Assert.That(motion.SampleDistance(motion.Duration + 1f), Is.EqualTo(2f));
        }

        [Test]
        public void TrapezoidalProfile_UsesCruiseAndReachesExactEndpoint()
        {
            ElevatorMotion motion = new(distance: 10f, maxSpeed: 3f, acceleration: 2f);

            Assert.That(motion.CruiseDuration, Is.GreaterThan(0f));
            Assert.That(motion.SampleDistance(motion.AccelerationDuration), Is.EqualTo(2.25f));
            Assert.That(motion.SampleDistance(motion.Duration), Is.EqualTo(10f));
        }
    }
}
#endif
