//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------


namespace UnityFrame{

	public static class DefineLayer
	{
		public const int Default = 0;

		public const int TransparentFX = 1;

		public const int IgoreRaycast = 2;

		public const int Water = 4;

		public const int UI = 5;

		public const int HitRaycast = 8;

        public const int Player = 9;
        public const int Monster = 10;


    }

    public static class DefineTag
    {
        public const string Untagged = "Untagged";

        public const string Wall = "Wall";

        public const string Block = "Block";

        public const string Unwalk = "Unwalk";

        public const string Walkable = "Walkable";

        public const string Ground = "Ground";

        public const string Gate = "Gate";

        public const string Player = "Player";

        public const string Monster = "Monster";

        //预定义
        public const string PlayerA = "PlayerA";
        public const string PlayerB = "PlayerB";
        public const string PlayerC = "PlayerC";
        public const string PlayerD = "PlayerD";
        

        //UI
        public const string IngoreLayout = "IngoreLayout";
    }

    public static class DefineTagMask
    {
        public const string Block = "Block;Ground;Gate";
        public const string Member = "Monster;Player";

    }




    //CS 中定义的事件类型
    public static class DefineEvent{
		public const int E_UI_OPERA = 0;

		public const int E_UI_SHOW = 1;

		public const int E_UI_CLOSE = 2;

		public const int E_LUA = 3;

		public const int E_NET_CONNECT_STATE = 4;

        public const int E_MAIN_PRE_START = 5;

        public const int E_MAIN_START = 6;

		public const int E_WEBPAGE_CLOSE = 11;

		public const int E_ANIMATION_CLIP = 100;

        public const int E_PERFORM_ACTION_CLIP = 106;


        public const int E_AVATAR_TRIGGER = 101;
        public const int E_AVATAR_COLLISION = 102;
        public const int E_DIP_COLLISION = 103;

        public const int E_TRIGGER_CONTROLLER = 104;

        public const int E_AVATAR_BLOCK = 105;

        public const int E_CLIENT_FIX = 1000;

		//FrameHandle 携程调用状态
		public const int E_COROUTINE_STATE = 2000;

		//vendor SDK调用
		public const int E_VENDOR_SDK = 3000;


        public const int E_ROOM_INFO_UPDATE = 4000;
        public const int E_START_BATTLE = 4001;
        public const int E_StartFrameSync = 4002;
        public const int E_StopFrameSync = 4003;
        public const int E_CLIENT_MSG = 4004; //客户端消息

    }


    //Lua 中定义的事件类型
    public static class DefineLuaEvent {
		public const string E_INPUT_DOWN = "E_INPUT_DOWN";	// 按下
		public const string E_INPUT_UP = "E_INPUT_UP";		// 弹起
		public const string E_INPUT_ESCAPE = "E_INPUT_ESCAPE";	// 按下退出

		public const string E_CUSTOM_IMAGE_RESPONSE = "E_CUSTOM_IMAGE_RESPONSE";	//自定义图片上传
		public const string E_CUSTOM_ICON_RESPONSE = "E_CUSTOM_ICON_RESPONSE";	//自定义icon上传

		public const string E_CONNECTION_STATE_CHANGE = "E_CONNECTION_STATE_CHANGE";	//连接状态变更

		public const string E_ANIMATION_CLIP = "E_ANIMATION_CLIP";	//Avatar 动画事件

        public const string E_PERFORM_ACTION_CLIP = "E_PERFORM_ACTION_CLIP";	//Perform 事件
        

        public const string E_CS_COROUTINE_STATE = "E_CS_COROUTINE_STATE";		//协程状态

		public const string E_VIEW_ON_SHOW = "E_VIEW_ON_SHOW";

		public const string E_VIEW_ON_CLOSE = "E_VIEW_ON_CLOSE";

		public const string E_WEBPAGE_ON_CLOSE = "E_WEBPAGE_ON_CLOSE";

		public const string E_APP_BACKGROUND_STATE = "E_APP_BACKGROUND_STATE";	//应用程序是否切换后台

		public const string E_VOICE_STATE = "E_VOICE_STATE";

		public const string E_RAYCAST_HIT = "E_RAYCAST_HIT";

        public const string E_DEBUG_GM = "E_DEBUG_GM";

        
        public const string E_DIP_COLLISION = "E_DIP_COLLISION";    //dip 碰撞

        public const string E_BIP_TRIGGER_ENTER = "E_BIP_TRIGGER_ENTER";    //dip 碰撞
        public const string E_BIP_TRIGGER_EXIT =  "E_BIP_TRIGGER_EXIT";    //dip 碰撞

        public const string E_AVATAR_TRIGGER = "E_AVATAR_TRIGGER";    //Avatar 触发器

        public const string E_AVATAR_COLLISION = "E_AVATAR_COLLISION";    //Avatar 碰撞

        public const string E_AVATAR_BLOCK = "E_AVATAR_BLOCK";    //Avatar 碰撞

        public const string E_LOGIN_HACK = "E_LOGIN_HACK";    //黑科技登陆



    }

}
