using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

    [CustomPropertyDrawer(typeof(KnobAttribute))]
    public class KnobDrawer : PropertyDrawer
    {
        Rect rect;
        static Vector2 mpos, lpos = Vector2.zero;
        Vector2 initPos;
        bool dragEnabled = false;
        KnobAttribute attr;
        delegate void dragdelegate();
        dragdelegate dragupdate;
        float knobangle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int id = GUIUtility.GetControlID(FocusType.Passive, position);

            KnobAttribute angle = attribute as KnobAttribute;
            rect = position;


            if (Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (position.Contains(mpos))
                        {
                            GUIUtility.hotControl = id;
                            lpos = initPos = GetMousePosition();
                            dragEnabled = true;
                            SetDragType();
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragEnabled)
                        {
                            dragupdate();
                            lpos = mpos;
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragEnabled)
                        {
                            GUIUtility.hotControl = -1;
                            dragEnabled = false;
                            SetValue(property);
                            property.serializedObject.ApplyModifiedProperties();
                        }
                        break;
                }
            }

        }

        void SetDragType()
        {
            switch (attr.dragType)
            {
                case KnobAttribute.DragType.Directional:
                    dragupdate = DragDirectional;
                    break;
                case KnobAttribute.DragType.Distance:
                    dragupdate = DragDistance;
                    break;
                case KnobAttribute.DragType.Horizontal:
                    dragupdate = DragHorizontal;
                    break;
                case KnobAttribute.DragType.Vertical:
                    dragupdate = DragVertical;
                    break;
                default:
                    dragupdate = DragDirectional;
                    break;
            }
        }
        void DragDirectional()
        {
            knobangle = Mathf.Clamp(Vector2.Angle(rect.center, mpos) - attr.centerAngle, attr.minAngle, attr.maxAngle) + attr.centerAngle;
        }
        void DragDistance()
        {
            knobangle = Mathf.Min(Vector2.Distance(initPos, mpos) * attr.delta * attr.angleRange, attr.angleRange) - attr.minAngle;
        }
        void DragHorizontal()
        {
            knobangle = Mathf.Clamp(knobangle + (mpos.x - initPos.x) * attr.delta, attr.minValue, attr.maxValue);
        }
        void DragVertical()
        {
            knobangle = Mathf.Clamp(knobangle + (mpos.x - initPos.x) * attr.delta, attr.minValue, attr.maxValue);
        }

        void SetValue(SerializedProperty property)
        {
            float v = (knobangle - attr.minAngle) / attr.angleRange * attr.valueRange;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Utility.AngleToVector2(v);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = (int)v;
                    break;

                case SerializedPropertyType.Float:
                default:
                    property.floatValue = v;
                    break;
            }
        }




        Vector2 GetMousePosition()
        { return mpos = Event.current.mousePosition; }
        bool PointInRange(float range, Vector2 point, Vector2 mousePos, out float range_out)
        { return (range_out = Vector2.Distance(point, mousePos)) <= range; }

    }


    public class KnobAttribute : PropertyAttribute
    {
        public enum DragType { Directional = 0, Distance, Horizontal, Vertical }
        public readonly DragType dragType;
        public readonly float minAngle, maxAngle, centerAngle, minValue, maxValue, angleRange, valueRange, delta;
        public KnobAttribute(float delta = 0.01f, float minValue = -1f, float maxValue = 1f, float centerAngle = -90f, float minAngle = -135f, float maxAngle = 135f, DragType dragType = 0)
        {
            this.delta = delta;
            this.dragType = dragType;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.centerAngle = centerAngle;
            this.minAngle = minAngle;
            this.maxAngle = maxAngle;
            valueRange = maxValue - minValue;
            angleRange = maxAngle - minAngle;
        }
    }
