using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Manager;
using _2Scripts.UI;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
	where T : MonoBehaviour
{
	private static T _instance;
	protected virtual void Awake()
	{
		if(_instance != null && _instance.gameObject != gameObject)
		{
			Destroy(gameObject);
			return;
		}

		_instance = GetComponent<T>();
		DontDestroyOnLoad(_instance);
	}
	
	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject(typeof(T).Name + " " + nameof(Singleton<T>)).AddComponent<T>();
			}
			
			if (typeof(T) == typeof(HUD) || typeof(T) == typeof(MultiManager))
			{
				Debug.Log($"CALL {typeof(T).Name}");
			}
			
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	protected virtual void OnDestroy()
	{
		// if(_instance.gameObject == gameObject)
		// 	_instance = null;
	}
}
