using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;

using BindType = ToLuaMenu.BindType;
using System.Reflection;

public static class CustomSettings
{
    public static string saveDir = Application.dataPath + "/Scripts/To"+"Lua/Source/Generate/";
	public static string toluaBaseType = Application.dataPath + "/Scripts/To"+"Lua/BaseType/";    

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.QualitySettings),
        typeof(UnityEngine.GL),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action)),                
        _DT(typeof(UnityEngine.Events.UnityAction)),
        _DT(typeof(System.Predicate<int>)),
        _DT(typeof(System.Action<int>)),
        _DT(typeof(System.Comparison<int>)),
		_DT(typeof(UnityFrame.DelegateMethod)),

    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {                
        //------------------------为例子导出--------------------------------
        //_GT(typeof(TestEventListener)),
        //_GT(typeof(TestProtol)),
        //_GT(typeof(TestAccount)),
        //_GT(typeof(Dictionary<int, TestAccount>)).SetLibName("AccountMap"),
        //_GT(typeof(KeyValuePair<int, TestAccount>)),    
        //_GT(typeof(TestExport)),
        //_GT(typeof(TestExport.Space)),
        //-------------------------------------------------------------------        
                
        //_GT(typeof(Debugger)).SetNameSpace(null),        

		_GT(typeof(Component)),
		_GT(typeof(Transform)),
		_GT(typeof(RectTransform)),
		_GT(typeof(Material)),
		//        _GT(typeof(Light)),
		//_GT(typeof(Rigidbody)),
		_GT(typeof(Camera)),
		//        _GT(typeof(AudioSource)),
		//_GT(typeof(LineRenderer))
		//_GT(typeof(TrailRenderer))

        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),        
        _GT(typeof(GameObject)),
//        _GT(typeof(TrackedReference)),
        //_GT(typeof(Application)),
        //_GT(typeof(Physics)),
        _GT(typeof(Collider)),
        //_GT(typeof(Time)),        
        _GT(typeof(Texture)),
        _GT(typeof(Texture2D)),
//        _GT(typeof(Shader)),        
        _GT(typeof(Renderer)),
//        _GT(typeof(WWW)),
        _GT(typeof(Screen)),        
//        _GT(typeof(CameraClearFlags)),
//        _GT(typeof(AudioClip)),        
//        _GT(typeof(AssetBundle)),
//        _GT(typeof(ParticleSystem)),
//        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)),        
//        _GT(typeof(LightType)),
//        _GT(typeof(SleepTimeout)),
#if UNITY_5_3_OR_NEWER
//        _GT(typeof(UnityEngine.Experimental.Director.DirectorPlayer)),
#endif
//        _GT(typeof(Animator)),
//        _GT(typeof(Input)),
//        _GT(typeof(KeyCode)),
//        _GT(typeof(SkinnedMeshRenderer)),
//        _GT(typeof(Space)),      
       

//        _GT(typeof(MeshRenderer)),
//#if !UNITY_5_4_OR_NEWER
//        _GT(typeof(ParticleEmitter)),
//        _GT(typeof(ParticleRenderer)),
//        _GT(typeof(ParticleAnimator)), 
//#endif
                              
        //_GT(typeof(BoxCollider)),
        //_GT(typeof(MeshCollider)),
        //_GT(typeof(SphereCollider)),        
        _GT(typeof(CharacterController)),
        //_GT(typeof(CapsuleCollider)),
        
//        _GT(typeof(Animation)),        
//        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)),        
//        _GT(typeof(AnimationState)),
//        _GT(typeof(AnimationBlendMode)),
//        _GT(typeof(QueueMode)),  
//        _GT(typeof(PlayMode)),
//        _GT(typeof(WrapMode)),
//
        _GT(typeof(QualitySettings)),
//        _GT(typeof(RenderSettings)),                                                   
//        _GT(typeof(BlendWeights)),           
//        _GT(typeof(RenderTexture)),
//        _GT(typeof(Resources)),

        _GT(typeof(SpriteRenderer)),
        _GT(typeof(Sprite)),
        
		//UnityFrame
		_GT(typeof(UnityFrame.MathX)),
        _GT(typeof(UnityFrame.JsonConvert)),
        _GT(typeof(UnityFrame.EntityObject)),

		//UI
		_GT(typeof(UnityFrame.UIObject)),
		_GT(typeof(UnityFrame.UIUpdateGroup)),


		//_GT(typeof(UnityFrame.UIAtlas)),
		_GT(typeof(UnityFrame.UISprite)),
		_GT(typeof(UnityFrame.UISpriteAnimation)),
		_GT(typeof(UnityFrame.UILabel)),
		_GT(typeof(UnityFrame.UIButton)),
		_GT(typeof(UnityFrame.UIGrid)),
		_GT(typeof(UnityFrame.UIFixedGrid)),
		_GT(typeof(UnityFrame.UIRecycleGrid)),
		_GT(typeof(UnityFrame.UIInputField)),
		_GT(typeof(UnityFrame.UIScrollView)),
		_GT(typeof(UnityFrame.UISlider)),
		_GT(typeof(UnityFrame.UITexture)),
		_GT(typeof(UnityFrame.UIToggle)),
		_GT(typeof(UnityFrame.UIRollGroup)),
		_GT(typeof(UnityFrame.UIToggleGroup)),
		_GT(typeof(UnityFrame.UIModel)),
		_GT(typeof(UnityFrame.UIFollow)),
		_GT(typeof(UnityFrame.UIDropdown)),
		_GT(typeof(UnityFrame.UIClock)),
		_GT(typeof(UnityFrame.UIScrollSelection)),
		_GT(typeof(UnityFrame.UIContent)),
		_GT(typeof(UnityFrame.UILineRender)),
		_GT(typeof(UnityFrame.UIDrag)),
		_GT(typeof(UnityFrame.UIDragGroup)),
		_GT(typeof(UnityFrame.UIDrawBoard)),
		_GT(typeof(UnityFrame.UISwitchGroup)),
		_GT(typeof(UnityFrame.UIFloatGroup)),
		_GT(typeof(UnityFrame.UITypewriter)),
		_GT(typeof(UnityFrame.UIClickOpera)),
        _GT(typeof(UnityFrame.UIEffect)),
        _GT(typeof(UnityFrame.UIItem)),
		_GT(typeof(UnityFrame.UIView)),
        _GT(typeof(UnityFrame.UIPreview)),
        _GT(typeof(UnityFrame.UITweenGroup)),
        

		//fx
		_GT(typeof(UnityFrame.FXController)),
		_GT(typeof(UnityFrame.EffectBase)),
        _GT(typeof(UnityFrame.ETweenBase)),
        _GT(typeof(UnityFrame.ETweenPosition)),
        _GT(typeof(UnityFrame.ETweenEuler)),
        _GT(typeof(UnityFrame.ETweenScale)),
        _GT(typeof(UnityFrame.EChainLine)),
        
        _GT(typeof(UnityFrame.ETweenBezierPoint)),

        _GT(typeof(UnityFrame.UITweenPosition)),
        _GT(typeof(UnityFrame.UITweenEuler)),
        _GT(typeof(UnityFrame.UITweenScale)),
        _GT(typeof(UnityFrame.UITweenAlpha)),
        _GT(typeof(UnityFrame.UITweenColor)),
        _GT(typeof(UnityFrame.UITweenUV)),
        _GT(typeof(UnityFrame.UITweenPathPoint)),


        _GT(typeof(UnityFrame.UIJoystick)),

		//avatar
		_GT(typeof(UnityFrame.AvatarController)),
		_GT(typeof(UnityFrame.AvatarAnimator)),
		_GT(typeof(UnityFrame.AvatarCapsule)),
		_GT(typeof(UnityFrame.AvatarMarkPoint)),
		_GT(typeof(UnityFrame.AvatarMotion)),
		_GT(typeof(UnityFrame.AvatarRender)),


		//global
		_GT(typeof(UnityFrame.GlobalPath)),
		_GT(typeof(UnityFrame.GlobalSettings)),
		_GT(typeof(UnityFrame.AssetSystem)),
		_GT(typeof(UnityFrame.GTime)),

		_GT(typeof(UnityFrame.CEntitySystem)),


		_GT(typeof(UnityFrame.PDataManager)),
		_GT(typeof(UnityFrame.ShaderManager)),
		_GT(typeof(UnityFrame.AudioManager)),
		_GT(typeof(UnityFrame.UIManager)),
		_GT(typeof(UnityFrame.FXManager)),
        _GT(typeof(UnityFrame.PerformActionManager)),
        _GT(typeof(UnityFrame.MotionManager)),
		_GT(typeof(UnityFrame.VoiceManager)),
		_GT(typeof(UnityFrame.VendorSDK)),
        _GT(typeof(UnityFrame.NavigateManager)),
        _GT(typeof(UnityFrame.RaycastManager)),
        _GT(typeof(UnityFrame.CheckPointManager)),
        


        _GT(typeof(UnityFrame.SceneCamera)),
        _GT(typeof(UnityFrame.SceneMap)),
        _GT(typeof(UnityFrame.SceneElement)),
        _GT(typeof(UnityFrame.SceneTrap)),

        //dip
        _GT(typeof(UnityFrame.DipController)),
        _GT(typeof(UnityFrame.DipThrowController)),
        _GT(typeof(UnityFrame.DipParabolaController)),
        _GT(typeof(UnityFrame.DipLineController)),
        _GT(typeof(UnityFrame.DipRangeController)),
        _GT(typeof(UnityFrame.DipRayController)),
        _GT(typeof(UnityFrame.DipTrackController)),
        _GT(typeof(UnityFrame.DipExplodeController)),
        _GT(typeof(UnityFrame.DipCurveController)),
        


        _GT(typeof(UnityFrame.TriggerObject)),
        _GT(typeof(UnityFrame.TriggerSprite)),
        


    };

    public static List<Type> dynamicList = new List<Type>()
    {
//        typeof(MeshRenderer),
//#if !UNITY_5_4_OR_NEWER
//        typeof(ParticleEmitter),
//        typeof(ParticleRenderer),
//        typeof(ParticleAnimator),
//#endif
//
//        typeof(BoxCollider),
//        typeof(MeshCollider),
//        typeof(SphereCollider),
//        typeof(CharacterController),
//        typeof(CapsuleCollider),
//
//        typeof(Animation),
//        typeof(AnimationClip),
//        typeof(AnimationState),
//
//        typeof(BlendWeights),
//        typeof(RenderTexture),
//        typeof(Rigidbody),
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    
}
