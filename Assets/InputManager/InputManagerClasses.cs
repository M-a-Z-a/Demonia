using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManager;

public class InputManagerClasses : MonoBehaviour
{

    public class INPUT
    {
        string _name = "";
        static Regex parse_regex = default;
        public Regex parseRegex { get => parse_regex; }
        public string name { get => _name; protected set => _name = value; }
        protected INPUT()
        { }
        protected INPUT(string name)
        { _name = name; }

        public virtual string ToSaveData()
        { return default; }
    }


    public class InputKey : INPUT
    {
        protected float _initTime = 0, _releaseTime = 0;

        protected InputKey()
        { }
        protected InputKey(string name) : base(name)
        { }

        public bool down { get; protected set; }
        public int downInt { get => ToInt(down); }
        public bool hold { get; protected set; }
        public int holdInt { get => ToInt(hold); }
        public bool up { get; protected set; }
        public int upInt { get => ToInt(up); }
        public float initTime { get => _initTime; }
        public float releaseTime { get => _releaseTime; }
        public float holdTime { get => hold ? Time.time - _initTime : 0; }


        public virtual void Update()
        {  }

        int ToInt(bool b)
        { return Convert.ToInt32(b); }

        public int CompareTo(InputKey o)
        { return Compare(this, o); }
        public static int Compare(InputKey a, InputKey b)
        {
            if (a.downInt > b.downInt)
            { return 1; }
            if (a.downInt < b.downInt)
            { return -1; }
            return 0;
        }

        public static implicit operator int(InputKey input_key)
        { return input_key.holdInt; }
        public static implicit operator float(InputKey input_key)
        { return input_key.holdInt; }
        public static implicit operator bool(InputKey input_key)
        { return input_key.hold; }

    }

    public class InputKeyCode : InputKey
    {
        static Regex parse_regex = new Regex(@"^\s*InputKeyCode:\s*(?<name>\w+)\s*,\s*(?<keycode>\w+)\s*$");
        //public static Regex parseRegex { get => parse_regex; }
        KeyCode kc;

        protected InputKeyCode(KeyCode keycode)
        { kc = keycode; }
        public InputKeyCode(string name, KeyCode keycode) : base(name)
        { kc = keycode; }
        


        public override void Update()
        {
            down = Input.GetKeyDown(kc);
            hold = Input.GetKey(kc);
            up = Input.GetKeyUp(kc);
            if (down)
            {
                _initTime = Time.time;
                return;
            }
            if (up)
            { _releaseTime = Time.time; }
        }

        public override string ToSaveData()
        { return $"InputKeyCode:{name},{KeyCode2String(kc)}"; }
        public static bool FromSaveData(string saveData, out InputKeyCode inputKeyCode)
        {
            inputKeyCode = default;
            KeyCode kc = KeyCode.None;
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                kc = String2KeyCode(m.Groups["keycode"].Value);
                inputKeyCode = new InputKeyCode(m.Groups["name"].Value, kc);
            }
            return kc == KeyCode.None;
        }

        public static string KeyCode2String(KeyCode kc)
        { return Enum.GetName(typeof(KeyCode), kc); }
        public static KeyCode String2KeyCode(string keyCode)
        { return Enum.Parse<KeyCode>(keyCode, true); }
    }

    public class InputButton : InputKey
    {
        static Regex parse_regex = new Regex(@"^\s*InputButton:\s*(?<name>\w+)\s*,\s*(?<button>\w+)\s*$");
        //public static Regex parseRegex { get => parse_regex; }
        string _button;

        public InputButton(string button)
        {  _button = button; }
        public InputButton(string name, string button) : base(name)
        { _button = button; }

        public override void Update()
        {
            down = Input.GetButtonDown(_button);
            hold = Input.GetButton(_button);
            up = Input.GetButtonUp(_button);
            if (down)
            {
                _initTime = Time.time;
                return;
            }
            if (up)
            { _releaseTime = Time.time; }
        }
        public override string ToSaveData()
        { return $"InputButton:{name},{_button}"; }
        public static bool FromSaveData(string saveData, out InputButton inputButton)
        {
            string btn = "";
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                btn = m.Groups["button"].Value;
                inputButton = new InputButton(m.Groups["name"].Value, btn);
            }
            inputButton = default;
            return btn != "";
        }

        
    }

    public class InputDirection : INPUT
    {
        static Regex parse_regex = new Regex(@"^\s*InputDirection:\s*(?<name>)\s*,\s*(?<positive>\w+)\s*-\s*(?<negative>\w+)\s*$");

        InputKey _positive, _negative;
        public int value { get => GetIntValue(); }
        public InputKey positive { get => _positive; }
        public InputKey negative { get => _negative; }

        public InputDirection(InputKey positive, InputKey negative)
        { _positive = positive; _negative = negative; }
        public InputDirection(string name, InputKey positive, InputKey negative) : base(name)
        { _positive = positive; _negative = negative; }

        int GetIntValue()
        { return _positive.holdInt - _negative.holdInt; }

        public static explicit operator string(InputDirection v)
        { return $"{v.value}"; }
        public static implicit operator int(InputDirection v)
        { return v.value; }
        public static implicit operator float(InputDirection v)
        { return v.value; }
        public static implicit operator bool(InputDirection v)
        { return v.value != 0; }

        public override string ToSaveData()
        { return $"InputDirection:{name},{_negative.name}-{_positive.name}"; }
        public static bool FromSaveData(string saveData, out InputDirection inputDirection)
        {
            inputDirection = default;
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                if (!TryGetInputKey<InputKey>(m.Groups["positive"].Value, out InputKey ipos)) return false;
                if (!TryGetInputKey<InputKey>(m.Groups["negative"].Value, out InputKey ineg)) return false;

                inputDirection = new InputDirection(m.Groups["name"].Value, ipos, ineg);
                return true;
            }
            return false;
        }
    }

    public class InputVector : INPUT
    {
        static Regex parse_regex = new Regex(@"^\s*InputVector:\s*(?<name>)\s*,\s*\[\s*(?<dimensions>(\b)+)\s*\]\s*$");
        

        protected delegate Vector2 Vector2Delegate();
        protected delegate Vector3 Vector3Delegate();

        protected Vector2Delegate toVector2;
        protected Vector3Delegate toVector3;

        protected InputDirection[] dims;
        public int Length { get => dims.Length; }
        
        public Vector2 vec2 { get => (Vector2)this; }
        public Vector3 vec3 { get => (Vector3)this; }
        public InputDirection GetAxisInput(int axis)
        { return dims[axis]; }

        public InputVector(params InputDirection[] dimensions)
        { 
            dims = dimensions;
            SetVectorDelegates();
        }
        public InputVector(string name, params InputDirection[] dimensions) : base(name)
        {
            dims = dimensions;
            SetVectorDelegates();
        }

        protected virtual void SetVectorDelegates()
        {
            if (dims.Length > 2)
            {
                toVector2 = () => { return new Vector2(dims[0].value, dims[1].value); };
                toVector3 = () => { return new Vector3(dims[0].value, dims[1].value, dims[2].value); };
                return;
            }
            if (dims.Length > 1)
            {
                toVector2 = () => { return new Vector2(dims[0].value, dims[1].value); };
                toVector3 = () => { return new Vector3(dims[0].value, dims[1].value, 0); };
                return;
            }
            if (dims.Length > 0)
            {
                toVector2 = () => { return new Vector2(dims[0].value, 0); };
                toVector3 = () => { return new Vector3(dims[0].value, 0, 0); };
                return;
            }
            toVector2 = () => { return Vector2.zero; };
            toVector3 = () => { return Vector3.zero; };
        }
        
        string GetVectorString()
        {
            List<string> slist = new();
            for (int i = 0; i < dims.Length; i++)
            { slist.Add((string)dims[i]); }
            return $"[{String.Join(',', slist)}]";
        }

        public override string ToSaveData()
        {
            List<string> s = new();
            foreach (InputDirection idir in dims)
            { s.Add(idir.name); }
            return $"InputVector:{name},[{string.Join(',',s)}]";
        }
        public static bool FromSaveData(string saveData, out InputVector inputVector)
        {
            inputVector = default;
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                List<InputDirection> idirs = new();
                InputDirection idir;
                int caplen = m.Groups["dimensions"].Captures.Count;
                foreach (string d in m.Groups["dimensions"].Captures)
                {
                    if (!TryGetInputDirection(d, out idir)) continue;
                    idirs.Add(idir);
                }

                Debug.Log($"Valid captures: {idirs.Count} / {caplen}");
                if (idirs.Count < caplen) return false;
                inputVector = new InputVector(idirs.ToArray());
            }
            return false;
        }

        public static explicit operator string(InputVector v)
        { return v.GetVectorString(); }
        public static explicit operator Vector2(InputVector v)
        { return v.toVector2(); }
        public static explicit operator Vector3(InputVector v)
        { return v.toVector3(); }

        public float this[int d]
        { get => dims[d].value; }
    }

    public class InputVector2 : InputVector
    {
        static Regex parse_regex = new Regex(@"^\s*InputVector2:\s*(?<name>)\s*,\s*\[\s*(?<x>\w+)\s*,\s*(?<y>\w+)\s*\]\s*$");

        public InputDirection inputX { get => dims[0]; }
        public InputDirection inputY { get => dims[1]; }
        public float x { get => this[0]; }
        public float y { get => this[1]; }
        public InputVector2(InputDirection x, InputDirection y) : base(x, y)
        { }
        public InputVector2(string name, InputDirection x, InputDirection y) : base(name, x, y)
        { }

        public override string ToSaveData()
        {
            return $"InputVector2:{name},[{dims[0].name},{dims[1].name}]";
        }
        public static bool FromSaveData(string saveData, out InputVector2 inputVector2)
        {
            inputVector2 = default;
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                if (!TryGetInputDirection(m.Groups["x"].Value, out InputDirection idirx)) return false;
                if (!TryGetInputDirection(m.Groups["y"].Value, out InputDirection idiry)) return false;

                inputVector2 = new InputVector2(m.Groups["name"].Value, idirx, idiry);
                return true;
            }
            return false;
        }

        protected override void SetVectorDelegates()
        {
            toVector2 = () => { return new Vector2(dims[0].value, dims[1].value); };
            toVector3 = () => { return new Vector3(dims[0].value, dims[1].value, 0); };
        }

    }

    public class InputVector3 : InputVector
    {
        static Regex parse_regex = new Regex(@"^\s*InputVector3:\s*(?<name>)\s*,\s*\[\s*(?<x>\w+)\s*,\s*(?<y>\w+)\s*,\s*(?<z>\w+)\s*\]\s*$");
        public InputDirection inputX { get => dims[0]; }
        public InputDirection inputY { get => dims[1]; }
        public InputDirection inputZ { get => dims[2]; }
        public float x { get => this[0]; }
        public float y { get => this[1]; }
        public float z { get => this[2]; }
        public InputVector3(InputDirection x, InputDirection y, InputDirection z) : base(x, y, z)
        { }
        public InputVector3(string name, InputDirection x, InputDirection y, InputDirection z) : base(name, x, y, z)
        { }

        public override string ToSaveData()
        { return $"InputVector3:{name},[{dims[0].name},{dims[1].name},{dims[2].name}]"; }
        public static bool FromSaveData(string saveData, out InputVector3 inputVector3)
        {
            inputVector3 = default;
            Match m = parse_regex.Match(saveData);
            if (m.Success)
            {
                if (!TryGetInputDirection(m.Groups["x"].Value, out InputDirection idirx)) return false;
                if (!TryGetInputDirection(m.Groups["y"].Value, out InputDirection idiry)) return false;
                if (!TryGetInputDirection(m.Groups["y"].Value, out InputDirection idirz)) return false;

                inputVector3 = new InputVector3(m.Groups["name"].Value, idirx, idiry, idirz);
                return true;
            }
            return false;
        }
        protected override void SetVectorDelegates()
        {
            toVector2 = () => { return new Vector2(dims[0].value, dims[1].value); };
            toVector3 = () => { return new Vector3(dims[0].value, dims[1].value, dims[2].value); };
        }
    }

}
