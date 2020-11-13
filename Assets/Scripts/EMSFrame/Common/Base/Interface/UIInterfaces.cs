//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityFrame
{

	public interface IUIUpdate
	{
		string updateKey{get;set;}
		RectTransform rectTransform{ get;}
		void UF_SetActive (bool active);
		void UF_SetValue(object value);
	}

	public interface IUIUpdateGroup : IUIUpdate
	{
		IUIUpdate UF_GetUI (string key);
		bool UF_SetKActive (string key,bool value);
		bool UF_SetKValue(string key,object value);
	}


	public interface IUIColorable{
		void UF_SetGrey(bool opera);
		void UF_SetAlpha (float value);
		void UF_SetColor (UnityEngine.Color value);
	}


    public interface IUILayout{
        RectTransform rectTransform { get; }
        void UF_RebuildLoyout();
        bool IsDestroyed();
    }

    public interface ISortingOrder {
        int sortingOrder { get; set; }
        bool isActiveAndEnabled { get; }
    }

    public interface ISortingRoot: ISortingOrder{
        bool isSortingValidate { get; }
    }


	public interface IUISetValue<T>
	{
		void UF_SetValue (T value);
	}

	public interface IUISetKValue<T>
	{
		bool UF_SetKValue (T value);
	}

}

