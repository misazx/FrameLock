﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using System.Collections.Generic;
using LuaInterface;

public static class DelegateFactory
{
	public delegate Delegate DelegateValue(LuaFunction func, LuaTable self, bool flag);
	public static Dictionary<Type, DelegateValue> dict = new Dictionary<Type, DelegateValue>();

	static DelegateFactory()
	{
		Register();
	}

	[NoToLuaAttribute]
	public static void Register()
	{
		dict.Clear();
		dict.Add(typeof(System.Action), System_Action);
		dict.Add(typeof(UnityEngine.Events.UnityAction), UnityEngine_Events_UnityAction);
		dict.Add(typeof(System.Predicate<int>), System_Predicate_int);
		dict.Add(typeof(System.Action<int>), System_Action_int);
		dict.Add(typeof(System.Comparison<int>), System_Comparison_int);
		dict.Add(typeof(UnityFrame.DelegateMethod), UnityFrame_DelegateMethod);
		dict.Add(typeof(UnityEngine.RectTransform.ReapplyDrivenProperties), UnityEngine_RectTransform_ReapplyDrivenProperties);
		dict.Add(typeof(UnityEngine.Camera.CameraCallback), UnityEngine_Camera_CameraCallback);
		dict.Add(typeof(UnityFrame.DelegateUIResposition), UnityFrame_DelegateUIResposition);
		dict.Add(typeof(UnityEngine.UI.InputField.OnValidateInput), UnityEngine_UI_InputField_OnValidateInput);
		dict.Add(typeof(UnityFrame.DelegateBoolMethod), UnityFrame_DelegateBoolMethod);
		dict.Add(typeof(UnityFrame.DelegateVoid), UnityFrame_DelegateVoid);
		dict.Add(typeof(UnityFrame.DelegateObject), UnityFrame_DelegateObject);
		dict.Add(typeof(UnityFrame.DelegateVectorMethod), UnityFrame_DelegateVectorMethod);
		dict.Add(typeof(UnityFrame.DelegateNForeach<UnityFrame.IEntityHnadle>), UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle);
	}

    [NoToLuaAttribute]
    public static Delegate CreateDelegate(Type t, LuaFunction func = null)
    {
        DelegateValue Create = null;

        if (!dict.TryGetValue(t, out Create))
        {
            throw new LuaException(string.Format("Delegate {0} not register", LuaMisc.GetTypeName(t)));            
        }

        if (func != null)
        {
            GLuaState state = func.GetGLuaState();
            LuaDelegate target = state.GetLuaDelegate(func);
            
            if (target != null)
            {
                return Delegate.CreateDelegate(t, target, target.method);
            }  
            else
            {
                Delegate d = Create(func, null, false);
                target = d.Target as LuaDelegate;
                state.AddLuaDelegate(target, func);
                return d;
            }       
        }

        return Create(func, null, false);        
    }

    [NoToLuaAttribute]
    public static Delegate CreateDelegate(Type t, LuaFunction func, LuaTable self)
    {
        DelegateValue Create = null;

        if (!dict.TryGetValue(t, out Create))
        {
            throw new LuaException(string.Format("Delegate {0} not register", LuaMisc.GetTypeName(t)));
        }

        if (func != null)
        {
            GLuaState state = func.GetGLuaState();
            LuaDelegate target = state.GetLuaDelegate(func, self);

            if (target != null)
            {
                return Delegate.CreateDelegate(t, target, target.method);
            }
            else
            {
                Delegate d = Create(func, self, true);
                target = d.Target as LuaDelegate;
                state.AddLuaDelegate(target, func, self);
                return d;
            }
        }

        return Create(func, self, true);
    }

    [NoToLuaAttribute]
    public static Delegate RemoveDelegate(Delegate obj, LuaFunction func)
    {
        GLuaState state = func.GetGLuaState();
        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld.func == func)
            {
                obj = Delegate.Remove(obj, ds[i]);
                state.DelayDispose(ld.func);
                break;
            }
        }

        return obj;
    }

    [NoToLuaAttribute]
    public static Delegate RemoveDelegate(Delegate obj, Delegate dg)
    {
        LuaDelegate remove = dg.Target as LuaDelegate;

        if (remove == null)
        {
            obj = Delegate.Remove(obj, dg);
            return obj;
        }

        GLuaState state = remove.func.GetGLuaState();
        Delegate[] ds = obj.GetInvocationList();        

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld == remove)
            {
                obj = Delegate.Remove(obj, ds[i]);
                state.DelayDispose(ld.func);
                state.DelayDispose(ld.self);
                break;
            }
        }

        return obj;
    }

	class System_Action_Event : LuaDelegate
	{
		public System_Action_Event(LuaFunction func) : base(func) { }
		public System_Action_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call()
		{
			func.Call();
		}

		public void CallWithSelf()
		{
			func.BeginPCall();
			func.Push(self);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate System_Action(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			System.Action fn = delegate() { };
			return fn;
		}

		if(!flag)
		{
			System_Action_Event target = new System_Action_Event(func);
			System.Action d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			System_Action_Event target = new System_Action_Event(func, self);
			System.Action d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityEngine_Events_UnityAction_Event : LuaDelegate
	{
		public UnityEngine_Events_UnityAction_Event(LuaFunction func) : base(func) { }
		public UnityEngine_Events_UnityAction_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call()
		{
			func.Call();
		}

		public void CallWithSelf()
		{
			func.BeginPCall();
			func.Push(self);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_Events_UnityAction(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityEngine.Events.UnityAction fn = delegate() { };
			return fn;
		}

		if(!flag)
		{
			UnityEngine_Events_UnityAction_Event target = new UnityEngine_Events_UnityAction_Event(func);
			UnityEngine.Events.UnityAction d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityEngine_Events_UnityAction_Event target = new UnityEngine_Events_UnityAction_Event(func, self);
			UnityEngine.Events.UnityAction d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class System_Predicate_int_Event : LuaDelegate
	{
		public System_Predicate_int_Event(LuaFunction func) : base(func) { }
		public System_Predicate_int_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public bool Call(int param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			bool ret = func.CheckBoolean();
			func.EndPCall();
			return ret;
		}

		public bool CallWithSelf(int param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			bool ret = func.CheckBoolean();
			func.EndPCall();
			return ret;
		}
	}

	public static Delegate System_Predicate_int(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			System.Predicate<int> fn = delegate(int param0) { return false; };
			return fn;
		}

		if(!flag)
		{
			System_Predicate_int_Event target = new System_Predicate_int_Event(func);
			System.Predicate<int> d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			System_Predicate_int_Event target = new System_Predicate_int_Event(func, self);
			System.Predicate<int> d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class System_Action_int_Event : LuaDelegate
	{
		public System_Action_int_Event(LuaFunction func) : base(func) { }
		public System_Action_int_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(int param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(int param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate System_Action_int(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			System.Action<int> fn = delegate(int param0) { };
			return fn;
		}

		if(!flag)
		{
			System_Action_int_Event target = new System_Action_int_Event(func);
			System.Action<int> d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			System_Action_int_Event target = new System_Action_int_Event(func, self);
			System.Action<int> d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class System_Comparison_int_Event : LuaDelegate
	{
		public System_Comparison_int_Event(LuaFunction func) : base(func) { }
		public System_Comparison_int_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public int Call(int param0, int param1)
		{
			func.BeginPCall();
			func.Push(param0);
			func.Push(param1);
			func.PCall();
			int ret = (int)func.CheckNumber();
			func.EndPCall();
			return ret;
		}

		public int CallWithSelf(int param0, int param1)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.Push(param1);
			func.PCall();
			int ret = (int)func.CheckNumber();
			func.EndPCall();
			return ret;
		}
	}

	public static Delegate System_Comparison_int(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			System.Comparison<int> fn = delegate(int param0, int param1) { return 0; };
			return fn;
		}

		if(!flag)
		{
			System_Comparison_int_Event target = new System_Comparison_int_Event(func);
			System.Comparison<int> d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			System_Comparison_int_Event target = new System_Comparison_int_Event(func, self);
			System.Comparison<int> d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateMethod_Event : LuaDelegate
	{
		public UnityFrame_DelegateMethod_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateMethod_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(object param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(object param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateMethod(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateMethod fn = delegate(object param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateMethod_Event target = new UnityFrame_DelegateMethod_Event(func);
			UnityFrame.DelegateMethod d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateMethod_Event target = new UnityFrame_DelegateMethod_Event(func, self);
			UnityFrame.DelegateMethod d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityEngine_RectTransform_ReapplyDrivenProperties_Event : LuaDelegate
	{
		public UnityEngine_RectTransform_ReapplyDrivenProperties_Event(LuaFunction func) : base(func) { }
		public UnityEngine_RectTransform_ReapplyDrivenProperties_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(UnityEngine.RectTransform param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(UnityEngine.RectTransform param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_RectTransform_ReapplyDrivenProperties(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityEngine.RectTransform.ReapplyDrivenProperties fn = delegate(UnityEngine.RectTransform param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityEngine_RectTransform_ReapplyDrivenProperties_Event target = new UnityEngine_RectTransform_ReapplyDrivenProperties_Event(func);
			UnityEngine.RectTransform.ReapplyDrivenProperties d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityEngine_RectTransform_ReapplyDrivenProperties_Event target = new UnityEngine_RectTransform_ReapplyDrivenProperties_Event(func, self);
			UnityEngine.RectTransform.ReapplyDrivenProperties d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityEngine_Camera_CameraCallback_Event : LuaDelegate
	{
		public UnityEngine_Camera_CameraCallback_Event(LuaFunction func) : base(func) { }
		public UnityEngine_Camera_CameraCallback_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(UnityEngine.Camera param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(UnityEngine.Camera param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityEngine_Camera_CameraCallback(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityEngine.Camera.CameraCallback fn = delegate(UnityEngine.Camera param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityEngine_Camera_CameraCallback_Event target = new UnityEngine_Camera_CameraCallback_Event(func);
			UnityEngine.Camera.CameraCallback d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityEngine_Camera_CameraCallback_Event target = new UnityEngine_Camera_CameraCallback_Event(func, self);
			UnityEngine.Camera.CameraCallback d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateUIResposition_Event : LuaDelegate
	{
		public UnityFrame_DelegateUIResposition_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateUIResposition_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(UnityFrame.IUIUpdate param0, int param1, int param2)
		{
			func.BeginPCall();
			func.PushObject(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(UnityFrame.IUIUpdate param0, int param1, int param2)
		{
			func.BeginPCall();
			func.Push(self);
			func.PushObject(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateUIResposition(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateUIResposition fn = delegate(UnityFrame.IUIUpdate param0, int param1, int param2) { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateUIResposition_Event target = new UnityFrame_DelegateUIResposition_Event(func);
			UnityFrame.DelegateUIResposition d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateUIResposition_Event target = new UnityFrame_DelegateUIResposition_Event(func, self);
			UnityFrame.DelegateUIResposition d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityEngine_UI_InputField_OnValidateInput_Event : LuaDelegate
	{
		public UnityEngine_UI_InputField_OnValidateInput_Event(LuaFunction func) : base(func) { }
		public UnityEngine_UI_InputField_OnValidateInput_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public char Call(string param0, int param1, char param2)
		{
			func.BeginPCall();
			func.Push(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			char ret = (char)func.CheckNumber();
			func.EndPCall();
			return ret;
		}

		public char CallWithSelf(string param0, int param1, char param2)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.Push(param1);
			func.Push(param2);
			func.PCall();
			char ret = (char)func.CheckNumber();
			func.EndPCall();
			return ret;
		}
	}

	public static Delegate UnityEngine_UI_InputField_OnValidateInput(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityEngine.UI.InputField.OnValidateInput fn = delegate(string param0, int param1, char param2) { return '\0'; };
			return fn;
		}

		if(!flag)
		{
			UnityEngine_UI_InputField_OnValidateInput_Event target = new UnityEngine_UI_InputField_OnValidateInput_Event(func);
			UnityEngine.UI.InputField.OnValidateInput d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityEngine_UI_InputField_OnValidateInput_Event target = new UnityEngine_UI_InputField_OnValidateInput_Event(func, self);
			UnityEngine.UI.InputField.OnValidateInput d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateBoolMethod_Event : LuaDelegate
	{
		public UnityFrame_DelegateBoolMethod_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateBoolMethod_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(bool param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(bool param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateBoolMethod(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateBoolMethod fn = delegate(bool param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateBoolMethod_Event target = new UnityFrame_DelegateBoolMethod_Event(func);
			UnityFrame.DelegateBoolMethod d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateBoolMethod_Event target = new UnityFrame_DelegateBoolMethod_Event(func, self);
			UnityFrame.DelegateBoolMethod d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateVoid_Event : LuaDelegate
	{
		public UnityFrame_DelegateVoid_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateVoid_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call()
		{
			func.Call();
		}

		public void CallWithSelf()
		{
			func.BeginPCall();
			func.Push(self);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateVoid(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateVoid fn = delegate() { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateVoid_Event target = new UnityFrame_DelegateVoid_Event(func);
			UnityFrame.DelegateVoid d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateVoid_Event target = new UnityFrame_DelegateVoid_Event(func, self);
			UnityFrame.DelegateVoid d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateObject_Event : LuaDelegate
	{
		public UnityFrame_DelegateObject_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateObject_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(UnityEngine.Object param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(UnityEngine.Object param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateObject(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateObject fn = delegate(UnityEngine.Object param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateObject_Event target = new UnityFrame_DelegateObject_Event(func);
			UnityFrame.DelegateObject d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateObject_Event target = new UnityFrame_DelegateObject_Event(func, self);
			UnityFrame.DelegateObject d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateVectorMethod_Event : LuaDelegate
	{
		public UnityFrame_DelegateVectorMethod_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateVectorMethod_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public void Call(UnityEngine.Vector3 param0)
		{
			func.BeginPCall();
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}

		public void CallWithSelf(UnityEngine.Vector3 param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.Push(param0);
			func.PCall();
			func.EndPCall();
		}
	}

	public static Delegate UnityFrame_DelegateVectorMethod(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateVectorMethod fn = delegate(UnityEngine.Vector3 param0) { };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateVectorMethod_Event target = new UnityFrame_DelegateVectorMethod_Event(func);
			UnityFrame.DelegateVectorMethod d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateVectorMethod_Event target = new UnityFrame_DelegateVectorMethod_Event(func, self);
			UnityFrame.DelegateVectorMethod d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

	class UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event : LuaDelegate
	{
		public UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event(LuaFunction func) : base(func) { }
		public UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event(LuaFunction func, LuaTable self) : base(func, self) { }

		public bool Call(UnityFrame.IEntityHnadle param0)
		{
			func.BeginPCall();
			func.PushObject(param0);
			func.PCall();
			bool ret = func.CheckBoolean();
			func.EndPCall();
			return ret;
		}

		public bool CallWithSelf(UnityFrame.IEntityHnadle param0)
		{
			func.BeginPCall();
			func.Push(self);
			func.PushObject(param0);
			func.PCall();
			bool ret = func.CheckBoolean();
			func.EndPCall();
			return ret;
		}
	}

	public static Delegate UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle(LuaFunction func, LuaTable self, bool flag)
	{
		if (func == null)
		{
			UnityFrame.DelegateNForeach<UnityFrame.IEntityHnadle> fn = delegate(UnityFrame.IEntityHnadle param0) { return false; };
			return fn;
		}

		if(!flag)
		{
			UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event target = new UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event(func);
			UnityFrame.DelegateNForeach<UnityFrame.IEntityHnadle> d = target.Call;
			target.method = d.Method;
			return d;
		}
		else
		{
			UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event target = new UnityFrame_DelegateNForeach_UnityFrame_IEntityHnadle_Event(func, self);
			UnityFrame.DelegateNForeach<UnityFrame.IEntityHnadle> d = target.CallWithSelf;
			target.method = d.Method;
			return d;
		}
	}

}

