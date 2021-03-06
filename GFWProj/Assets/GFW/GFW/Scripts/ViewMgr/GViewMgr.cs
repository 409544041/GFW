﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GFW;

namespace GFW
{
	public enum GViewZOrder
	{
		kZOrderMinus1,
		kZOrder0,
		kZOrder1,
	}

	public enum GViewType
	{
		kUIView,
		kModalView
	}

	public delegate GameObject GFuncViewCreator ();
	public class GViewStack
	{
		private GViewZOrder zOrder_;
		private List<GFuncViewCreator> viewStack_ = new List<GFuncViewCreator> ();
		private GViewType viewType_;

		public GViewType ViewType {
			get{ return viewType_; }
		}

		public GViewStack (GViewZOrder zOrder, GViewType viewType)
		{
			this.zOrder_ = zOrder;
			this.viewType_ = viewType;
		}

		string wrapperNodeName = "__wrapper__";

		protected GameObject CreateWrapperNode (GameObject viewRoot)
		{
			var wrapper = GCoordUtility.CreateFullScreenUINode (viewRoot, wrapperNodeName);
			var imageComp = wrapper.AddComponent<Image> ();
			imageComp.color = new Color (0, 0, 0, 0);
			wrapper.transform.SetParent (viewRoot.transform);
			return wrapper;
		}

		public GameObject PushView (GFuncViewCreator createFunc)
		{
			if (createFunc != null) {
				var viewRoot = GSceneMgr.GetInstance ().GetViewRoot (viewType_, zOrder_);
				GUtility.RemoveAllChildren (viewRoot);
				var wrapper = CreateWrapperNode (viewRoot);
				var view = createFunc ();
				GLogUtility.LogDebug ("view name = " + view.name);
				view.transform.SetParent (wrapper.transform);

				GCoordUtility.ResetRectToFullScreenAndInMiddle (view.GetComponent<RectTransform> ());

				viewStack_.Add (createFunc);
				return view;
			}
			return null;
		}

		public void ShowTopView ()
		{
			GFuncViewCreator createFunc = null;
			if (viewStack_.Count > 0) {
				createFunc = viewStack_ [viewStack_.Count - 1];
			}
			var viewRoot = GSceneMgr.GetInstance ().GetViewRoot (viewType_, zOrder_);
			GUtility.RemoveAllChildren (viewRoot);
			if (createFunc != null) {
				var wrapper = CreateWrapperNode (viewRoot);
				var view = createFunc ();
				view.transform.SetParent (wrapper.transform);

				GCoordUtility.ResetRectToFullScreenAndInMiddle (view.GetComponent<RectTransform> ());
			}
		}

		public void PopView ()
		{
			GFuncViewCreator createFunc = null;
			if (viewStack_.Count > 1) {
				createFunc = viewStack_ [viewStack_.Count - 2];
			}
			var viewRoot = GSceneMgr.GetInstance ().GetViewRoot (viewType_, zOrder_);
			GUtility.RemoveAllChildren (viewRoot);
			if (createFunc != null) {
				var wrapper = CreateWrapperNode (viewRoot);
				var view = createFunc ();
				view.transform.SetParent (wrapper.transform);

				GCoordUtility.ResetRectToFullScreenAndInMiddle (view.GetComponent<RectTransform> ());
			}
			if (viewStack_.Count > 0) {
				viewStack_.RemoveAt (viewStack_.Count - 1);
			}
		}

		public void EmptyStack ()
		{
			var viewRoot = GSceneMgr.GetInstance ().GetViewRoot (viewType_, zOrder_);
			GUtility.RemoveAllChildren (viewRoot);
			viewStack_.Clear ();
		}
	}

	public class GViewStackGroup
	{
		private Dictionary<GViewZOrder,GViewStack> viewStackMap_;
		private GViewType viewType_;

		public GViewType ViewType {
			get{ return viewType_; }
		}

		public Dictionary<GViewZOrder,GViewStack> ViewStackMap {
			get{ return viewStackMap_; }
		}

		public GViewStackGroup (GViewType viewType)
		{
			viewType_ = viewType;
			viewStackMap_ = new Dictionary<GViewZOrder, GViewStack> ();
			foreach (GViewZOrder zOrder in (GViewZOrder[])GViewZOrder.GetValues(typeof(GViewZOrder))) {
				viewStackMap_.Add (zOrder, new GViewStack (zOrder, viewType_));
			}
		}
	}

	public class GViewMgrBase
	{
		protected GViewStackGroup curViewGroup;

		public GViewStackGroup CurViewGroup {
			set{ curViewGroup = value; }
		}

		public GameObject PushView (GFuncViewCreator createFunc, GViewZOrder zOrder = GViewZOrder.kZOrder0)
		{
			if (curViewGroup != null) {
				return curViewGroup.ViewStackMap [zOrder].PushView (createFunc);
			} else {
				GLogUtility.LogError ("curViewGroup == null");
				return null;
			}
		}

		public void ShowAllTopView ()
		{
			if (curViewGroup != null) {
				foreach (var kv in curViewGroup.ViewStackMap) {
					kv.Value.ShowTopView ();
				}
			} else {
				GLogUtility.LogError ("curViewGroup == null");
			}
		}

		public void PopView (GViewZOrder zOrder = GViewZOrder.kZOrder0)
		{
			if (curViewGroup != null) {
				curViewGroup.ViewStackMap [zOrder].PopView ();
			} else {
				GLogUtility.LogError ("curViewGroup == null");
			}
		}

		public void EmptyStack (GViewZOrder zOrder = GViewZOrder.kZOrder0)
		{
			if (curViewGroup != null) {
				curViewGroup.ViewStackMap [zOrder].EmptyStack ();
			} else {
				GLogUtility.LogError ("curViewGroup == null");
			}
		}

		public void EmptyAllStack ()
		{
			if (curViewGroup != null) {
				foreach (var kv in curViewGroup.ViewStackMap) {
					kv.Value.EmptyStack ();	
				}
			} else {
				GLogUtility.LogError ("curViewGroup == null");
			}
		}
	}
}