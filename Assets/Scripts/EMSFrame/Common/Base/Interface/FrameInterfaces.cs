//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;

namespace UnityFrame
{

    public interface IFrameHandle { }

    public interface IEntityHnadle
    {
       string name { get; set; }
       int eLayer { get; set; }
       bool isReleased { get; set; }
       bool isActive { get; set; }
       float timeStamp { get; set; }
    }

    public interface IReleasable
    {
        void Release();
    }

    public interface IOnAwake{
		void UF_OnAwake();
	}

	public interface IOnStart{
		void UF_OnStart();
	}

	public interface IOnUpdate
	{
		void UF_OnUpdate();
	}

    public interface IOnFixedUpdate
    {
        void UF_OnFixedUpdate();
    }

    public interface IOnSyncUpdate {
        void UF_OnSyncUpdate();
    }

	public interface IOnLateUpdate
	{
		void UF_OnLateUpdate();
	}

	public interface IOnSecnodUpdate
	{
		void UF_OnSecnodUpdate();
	}

	public interface IOnGUI {
		void OnGUI () ;
	}

	public interface IOnApplicationPause {
		void OnApplicationPause(bool state);
	}

	public interface IOnApplicationQuit {
		void OnApplicationQuit();
	}

	public interface IOnClick{
		void OnClick (UnityEngine.EventSystems.PointerEventData eventData);
	}

    public interface IOnDestroyed {
        void UF_OnDestroyed();
    }

    public interface IOnReset{
		void UF_OnReset();
	}
}

