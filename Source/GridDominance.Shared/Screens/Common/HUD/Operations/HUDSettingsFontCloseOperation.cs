﻿using MonoSAMFramework.Portable.Screens.HUD.Operations;
using MonoSAMFramework.Portable.Input;

namespace GridDominance.Shared.Screens.WorldMapScreen.HUD
{
	class HUDSettingsFontCloseOperation : HUDTimedElementOperation<SettingsButton>
	{
		private readonly int _index;

		private float startState;

		public HUDSettingsFontCloseOperation(int idx) : base(0.25f)
		{
			_index = idx;
		}

		protected override void OnStart(SettingsButton button)
		{
			startState = button.SubButtons[_index].FontProgress;
		}

		protected override void OnProgress(SettingsButton button, float progress, InputState istate)
		{
			button.SubButtons[_index].FontProgress = startState * (1 - progress);
		}

		protected override void OnEnd(SettingsButton button)
		{
			button.SubButtons[_index].Alive = false;
			button.SubButtons[_index].ScaleProgress = 0f;
		}

		public override string Name => "SettingsFontCloseOperation";
	}
}
