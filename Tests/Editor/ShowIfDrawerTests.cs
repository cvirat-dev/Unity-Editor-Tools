using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UUP.EditorTools.Editor.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="ShowIfDrawer"/>. The drawer is driven directly: the
    /// attribute is injected into a drawer instance and the private IsVisible method is
    /// invoked via reflection, so no Inspector window or GUI event is required.
    /// </summary>
    [TestFixture]
    public class ShowIfDrawerTests
    {
        private enum TestEnum
        {
            First,
            Second,
            Third
        }

        private sealed class ShowIfTestAsset : ScriptableObject
        {
            public bool condition;
            public int boolDriven;

            public Object reference;
            public int referenceDriven;

            public TestEnum enumValue;
            public int enumDriven;

            public int intValue;
            public int intDriven;

            public float floatValue;
            public int floatDriven;

            [NonSerialized] public bool reflectedCondition;
            public int reflectedDriven;

            public NestedData nested = new NestedData();

            public bool ReflectedProperty => reflectedCondition;
            public bool ReflectedMethod() => reflectedCondition;

            [Serializable]
            public sealed class NestedData
            {
                public bool condition; // shadows the root field on purpose
                public bool nestedCondition;
                public int nestedDriven;
                public int shadowedDriven;
            }
        }

        private static readonly FieldInfo s_AttributeField =
            typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo s_IsVisibleMethod =
            typeof(ShowIfDrawer).GetMethod("IsVisible", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo s_ReportedErrorsField =
            typeof(ShowIfDrawer).GetField("s_ReportedErrors", BindingFlags.Static | BindingFlags.NonPublic);

        private ShowIfTestAsset m_Asset;
        private SerializedObject m_SerializedObject;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(s_AttributeField, "PropertyDrawer.m_Attribute was not found; Unity may have renamed the backing field.");
            Assert.NotNull(s_IsVisibleMethod, "ShowIfDrawer.IsVisible was not found; update this harness if the drawer was refactored.");
            Assert.NotNull(s_ReportedErrorsField, "ShowIfDrawer.s_ReportedErrors was not found; update this harness if the drawer was refactored.");

            // The drawer reports each misconfiguration only once per session; reset so tests stay independent.
            ((HashSet<string>)s_ReportedErrorsField.GetValue(null)).Clear();

            m_Asset = ScriptableObject.CreateInstance<ShowIfTestAsset>();
            m_SerializedObject = new SerializedObject(m_Asset);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Asset);
        }

        private bool IsVisible(string propertyPath, ShowIfAttribute attribute)
        {
            SerializedProperty property = m_SerializedObject.FindProperty(propertyPath);
            Assert.NotNull(property, $"Property '{propertyPath}' was not found on the test asset.");

            var drawer = new ShowIfDrawer();
            s_AttributeField.SetValue(drawer, attribute);
            return (bool)s_IsVisibleMethod.Invoke(drawer, new object[] { property });
        }

        private void Refresh() => m_SerializedObject.Update();

        // --- Bool condition ---

        [Test]
        public void BoolCondition_WithoutCompareValue_VisibleOnlyWhileTrue()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.condition));

            m_Asset.condition = true;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute));

            m_Asset.condition = false;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute));
        }

        [Test]
        public void BoolCondition_WithExplicitCompareValue_VisibleOnlyOnMatch()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.condition), false);

            m_Asset.condition = false;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute));

            m_Asset.condition = true;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute));
        }

        // --- Object reference condition ---

        [Test]
        public void ObjectReference_WithoutCompareValue_VisibleOnlyWhileAssigned()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.reference));

            m_Asset.reference = null;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.referenceDriven), attribute));

            m_Asset.reference = m_Asset;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.referenceDriven), attribute));
        }

        [Test]
        public void ObjectReference_WithFalseCompareValue_VisibleOnlyWhileUnassigned()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.reference), false);

            m_Asset.reference = null;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.referenceDriven), attribute));

            m_Asset.reference = m_Asset;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.referenceDriven), attribute));
        }

        // --- Enum condition ---

        [Test]
        public void EnumCondition_VisibleOnlyOnMatchingValue()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.enumValue), TestEnum.Second);

            m_Asset.enumValue = TestEnum.First;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.enumDriven), attribute));

            m_Asset.enumValue = TestEnum.Second;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.enumDriven), attribute));

            m_Asset.enumValue = TestEnum.Third;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.enumDriven), attribute));
        }

        // --- Numeric conditions ---

        [Test]
        public void IntCondition_VisibleOnlyOnMatchingValue()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.intValue), 3);

            m_Asset.intValue = 2;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.intDriven), attribute));

            m_Asset.intValue = 3;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.intDriven), attribute));
        }

        [Test]
        public void FloatCondition_VisibleOnlyOnMatchingValue()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.floatValue), 2.5);

            m_Asset.floatValue = 2.5f;
            Refresh();
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.floatDriven), attribute));

            m_Asset.floatValue = 0f;
            Refresh();
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.floatDriven), attribute));
        }

        // --- Nested serializable sibling paths ---

        [Test]
        public void NestedCondition_ResolvesSiblingNextToControlledField()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.NestedData.nestedCondition));
            const string drivenPath = "nested.nestedDriven";

            m_Asset.nested.nestedCondition = true;
            Refresh();
            Assert.IsTrue(IsVisible(drivenPath, attribute));

            m_Asset.nested.nestedCondition = false;
            Refresh();
            Assert.IsFalse(IsVisible(drivenPath, attribute));
        }

        [Test]
        public void NestedCondition_PrefersNestedSiblingOverRootMemberWithSameName()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.NestedData.condition));
            const string drivenPath = "nested.shadowedDriven";

            // Root 'condition' is true: if the drawer wrongly resolved the root member, the field would show.
            m_Asset.condition = true;
            m_Asset.nested.condition = false;
            Refresh();
            Assert.IsFalse(IsVisible(drivenPath, attribute));

            m_Asset.nested.condition = true;
            Refresh();
            Assert.IsTrue(IsVisible(drivenPath, attribute));
        }

        // --- Reflected members (no serialized sibling exists) ---

        [Test]
        public void ReflectedProperty_UsedWhenNoSerializedSiblingExists()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.ReflectedProperty));

            // Reflected members are read from the live object, so no SerializedObject.Update() is needed.
            m_Asset.reflectedCondition = true;
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));

            m_Asset.reflectedCondition = false;
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));
        }

        [Test]
        public void ReflectedMethod_UsedWhenNoSerializedSiblingExists()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.ReflectedMethod));

            m_Asset.reflectedCondition = true;
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));

            m_Asset.reflectedCondition = false;
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));
        }

        [Test]
        public void NonSerializedField_UsedWhenNoSerializedSiblingExists()
        {
            var attribute = new ShowIfAttribute(nameof(ShowIfTestAsset.reflectedCondition));

            m_Asset.reflectedCondition = true;
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));

            m_Asset.reflectedCondition = false;
            Assert.IsFalse(IsVisible(nameof(ShowIfTestAsset.reflectedDriven), attribute));
        }

        // --- Invalid member name ---

        [Test]
        public void InvalidMemberName_LogsWarningOnceAndKeepsFieldVisible()
        {
            var attribute = new ShowIfAttribute("memberThatDoesNotExist");
            var expectedWarning = new Regex(@"No field, property or parameterless method named 'memberThatDoesNotExist'");

            LogAssert.Expect(LogType.Warning, expectedWarning);

            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute), "Field must remain visible on misconfiguration.");
            Assert.IsTrue(IsVisible(nameof(ShowIfTestAsset.boolDriven), attribute), "Field must remain visible on misconfiguration.");

            // Fails the test if the second call logged the warning again.
            LogAssert.NoUnexpectedReceived();
        }
    }
}
