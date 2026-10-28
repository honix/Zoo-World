using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Tests
{
    public class MovementSettingsTests
    {
        [Test]
        public void Defaults_AreValid()
        {
            Assert.That(Create<LinearMovementSettings>().Validate(), Is.Empty);
            Assert.That(Create<JumpMovementSettings>().Validate(), Is.Empty);
        }

        [Test]
        public void JumpLongerThanInterval_IsReported()
        {
            var settings = Create<JumpMovementSettings>();
            Set(settings, "_duration", 5f);

            Assert.That(settings.Validate(), Has.Some.Contains("shorter than the interval"));
        }

        [Test]
        public void ZeroSpeed_IsReported()
        {
            var settings = Create<LinearMovementSettings>();
            Set(settings, "_speed", 0f);

            Assert.That(settings.Validate(), Is.Not.Empty);
        }

        [Test]
        public void Settings_CreateTheirMovement()
        {
            var random = new SeededRandom(0);

            Assert.That(Create<LinearMovementSettings>().CreateMovement(random), Is.TypeOf<LinearMovement>());
            Assert.That(Create<JumpMovementSettings>().CreateMovement(random), Is.TypeOf<JumpMovement>());
        }

        private static T Create<T>() where T : ScriptableObject => ScriptableObject.CreateInstance<T>();

        private static void Set(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
