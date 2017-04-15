﻿using System;
using System.Linq;
using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens.ScreenGame;
using GridDominance.Shared.Screens.WorldMapScreen;
using GridDominance.Shared.Screens.WorldMapScreen.HUD;
using Microsoft.Xna.Framework.Graphics;
using MonoSAMFramework.Portable.DebugTools;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Screens.Entities.Particles;

namespace GridDominance.Shared.Screens
{
	public static class DebugUtils
	{
#if DEBUG
		public static DebugTextDisplay CreateDisplay(GameScreen scrn)
		{
			var debugDisp = new DebugTextDisplay(scrn.Graphics.GraphicsDevice, Textures.DebugFont);

			debugDisp.AddLine(() => $"Device = {scrn.Game.Bridge.DeviceName} | Version = {scrn.Game.Bridge.DeviceVersion}");
			debugDisp.AddLine(() => $"FPS={scrn.FPSCounter.AverageAPS:0000.0} (curr={scrn.FPSCounter.CurrentAPS:0000.0} delta={scrn.FPSCounter.AverageDelta * 1000:000.00} min={scrn.FPSCounter.MinimumAPS:0000.0} (d={scrn.FPSCounter.MaximumDelta * 1000:0000.0}) cycletime={scrn.FPSCounter.AverageCycleTime * 1000:000.00} (max={scrn.FPSCounter.MaximumCycleTime * 1000:000.00} curr={scrn.FPSCounter.CurrentCycleTime * 1000:000.00}) total={scrn.FPSCounter.TotalActions:000000})");
			debugDisp.AddLine(() => $"UPS={scrn.UPSCounter.AverageAPS:0000.0} (curr={scrn.UPSCounter.CurrentAPS:0000.0} delta={scrn.UPSCounter.AverageDelta * 1000:000.00} min={scrn.UPSCounter.MinimumAPS:0000.0} (d={scrn.UPSCounter.MaximumDelta * 1000:0000.0}) cycletime={scrn.UPSCounter.AverageCycleTime * 1000:000.00} (max={scrn.UPSCounter.MaximumCycleTime * 1000:000.00} curr={scrn.UPSCounter.CurrentCycleTime * 1000:000.00}) total={scrn.UPSCounter.TotalActions:000000})");
			debugDisp.AddLine(() => $"GC = Time since GC:{scrn.GCMonitor.TimeSinceLastGC:00.00}s ({scrn.GCMonitor.TimeSinceLastGC0:000.00}s | {scrn.GCMonitor.TimeSinceLastGC1:000.00}s | {scrn.GCMonitor.TimeSinceLastGC2:000.00}s) Memory = {scrn.GCMonitor.TotalMemory:000.0}MB Frequency = {scrn.GCMonitor.GCFrequency:0.000}");
			debugDisp.AddLine(() => $"Quality = {Textures.TEXTURE_QUALITY} | Texture.Scale={1f / Textures.DEFAULT_TEXTURE_SCALE.X:#.00} | Pixel.Scale={Textures.GetDeviceTextureScaling(scrn.Game.GraphicsDevice):#.00}");
			debugDisp.AddLine(() => $"Entities = {scrn.Entities.Count(),3} | EntityOps = {scrn.Entities.Enumerate().Sum(p => p.ActiveEntityOperations.Count()):00} | Particles = {scrn.Entities.Enumerate().OfType<IParticleOwner>().Sum(p => p.ParticleCount),3} (Visible: {scrn.Entities.Enumerate().Where(p => p.IsInViewport).OfType<IParticleOwner>().Sum(p => p.ParticleCount),3})");
			debugDisp.AddLine(() => $"GamePointer = ({scrn.InputStateMan.GetCurrentState().GamePointerPosition.X:000.0}|{scrn.InputStateMan.GetCurrentState().GamePointerPosition.Y:000.0}) | HUDPointer = ({scrn.InputStateMan.GetCurrentState().HUDPointerPosition.X:000.0}|{scrn.InputStateMan.GetCurrentState().HUDPointerPosition.Y:000.0}) | PointerOnMap = ({scrn.InputStateMan.GetCurrentState().GamePointerPositionOnMap.X:000.0}|{scrn.InputStateMan.GetCurrentState().GamePointerPositionOnMap.Y:000.0})");
			debugDisp.AddLine("DebugGestures", () => $"Pinching = {scrn.InputStateMan.GetCurrentState().IsGesturePinching} & PinchComplete = {scrn.InputStateMan.GetCurrentState().IsGesturePinchComplete} & PinchPower = {scrn.InputStateMan.GetCurrentState().LastPinchPower}");
			debugDisp.AddLine(() => $"OGL Sprites = {scrn.LastReleaseRenderSpriteCount:0000} (+ {scrn.LastDebugRenderSpriteCount:0000}); OGL Text = {scrn.LastReleaseRenderTextCount:0000} (+ {scrn.LastDebugRenderTextCount:0000})");
			debugDisp.AddLine(() => $"Map Offset = {scrn.MapOffset} (Map Center = {scrn.MapViewportCenter})");
			if (scrn is GDGameScreen)debugDisp.AddLine(() => $"LevelTime = {((GDGameScreen)scrn).LevelTime:000.000} (finished={((GDGameScreen)scrn).HasFinished})");

			if (scrn is GDWorldMapScreen) debugDisp.AddLine(() => $"CurrentLevelNode = {((GDWorldHUD)scrn.HUD).SelectedNode?.Level?.Name ?? "NULL"}; FocusedHUDElement = {scrn.HUD.FocusedElement}; ZoomState = {((GDWorldMapScreen)scrn).ZoomState}");

			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GraphicsDevice.Viewport=[{scrn.Game.GraphicsDevice.Viewport.Width}|{scrn.Game.GraphicsDevice.Viewport.Height}]");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.VirtualGuaranteedSize={scrn.VAdapterGame.VirtualGuaranteedSize} || GameAdapter.VirtualGuaranteedSize={scrn.VAdapterHUD.VirtualGuaranteedSize}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.RealGuaranteedSize={scrn.VAdapterGame.RealGuaranteedSize} || GameAdapter.RealGuaranteedSize={scrn.VAdapterHUD.RealGuaranteedSize}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.VirtualTotalSize={scrn.VAdapterGame.VirtualTotalSize} || GameAdapter.VirtualTotalSize={scrn.VAdapterHUD.VirtualTotalSize}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.RealTotalSize={scrn.VAdapterGame.RealTotalSize} || GameAdapter.RealTotalSize={scrn.VAdapterHUD.RealTotalSize}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.VirtualOffset={scrn.VAdapterGame.VirtualGuaranteedBoundingsOffset} || GameAdapter.VirtualOffset={scrn.VAdapterHUD.VirtualGuaranteedBoundingsOffset}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.RealOffset={scrn.VAdapterGame.RealGuaranteedBoundingsOffset} || GameAdapter.RealOffset={scrn.VAdapterHUD.RealGuaranteedBoundingsOffset}");
			debugDisp.AddLine("ShowMatrixTextInfos", () => $"GameAdapter.Scale={scrn.VAdapterGame.Scale} || GameAdapter.Scale={scrn.VAdapterHUD.Scale}");

			debugDisp.AddLine("ShowOperations", () => string.Join(Environment.NewLine, scrn.Entities.Enumerate().SelectMany(e => e.ActiveEntityOperations).Select(o => o.Name)));
			debugDisp.AddLine("ShowOperations", () => string.Join(Environment.NewLine, scrn.HUD.Enumerate().SelectMany(e => e.ActiveHUDOperations).Select(o => o.Name)));

			debugDisp.AddLine("ShowDebugShortcuts", DebugSettings.GetSummary);

			debugDisp.AddLogLines();

			debugDisp.AddLine("ShowSerializedProfile", () => MainGame.Inst.Profile.SerializeToString(128));

			debugDisp.AddLine("FALSE", () => scrn.InputStateMan.GetCurrentState().GetFullDebugSummary());
			debugDisp.AddLine("FALSE", () => scrn.Game.Bridge.FullDeviceInfoString);

			return debugDisp;
		}

		public static void CreateShortcuts(GameScreen scrn)
		{
			DebugSettings.AddSwitch(null, "DBG", scrn, KCL.C(SKeys.D, SKeys.AndroidMenu), true);

			DebugSettings.AddTrigger("DBG", "SetQuality_1", scrn, SKeys.D1, KeyModifier.Control, x => Textures.ChangeQuality(scrn.Game.Content, TextureQuality.FD));
			DebugSettings.AddTrigger("DBG", "SetQuality_2", scrn, SKeys.D2, KeyModifier.Control, x => Textures.ChangeQuality(scrn.Game.Content, TextureQuality.BD));
			DebugSettings.AddTrigger("DBG", "SetQuality_3", scrn, SKeys.D3, KeyModifier.Control, x => Textures.ChangeQuality(scrn.Game.Content, TextureQuality.LD));
			DebugSettings.AddTrigger("DBG", "SetQuality_4", scrn, SKeys.D4, KeyModifier.Control, x => Textures.ChangeQuality(scrn.Game.Content, TextureQuality.MD));
			DebugSettings.AddTrigger("DBG", "SetQuality_5", scrn, SKeys.D5, KeyModifier.Control, x => Textures.ChangeQuality(scrn.Game.Content, TextureQuality.HD));
			DebugSettings.AddTrigger("DBG", "ResetProfile", scrn, SKeys.R, KeyModifier.Control, x => MainGame.Inst.ResetProfile());

			if (scrn is GDWorldMapScreen) DebugSettings.AddTrigger("DBG", "ResetProfile", scrn, SKeys.Z, KeyModifier.None, x => ((GDWorldMapScreen)scrn).ZoomOut());

			DebugSettings.AddSwitch("DBG", "PhysicsDebugView",      scrn, SKeys.F1,  KeyModifier.None, false);
			DebugSettings.AddSwitch("DBG", "DebugTextDisplay",      scrn, SKeys.F2,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "DebugBackground",       scrn, SKeys.F3,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "DebugHUDBorders",       scrn, SKeys.F4,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "DebugCannonView",       scrn, SKeys.F5,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "ShowMatrixTextInfos",   scrn, SKeys.F6,  KeyModifier.None, false);
			DebugSettings.AddSwitch("DBG", "ShowDebugMiniMap",      scrn, SKeys.F7,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "DebugEntityBoundaries", scrn, SKeys.F8,  KeyModifier.None, true);
			DebugSettings.AddSwitch("DBG", "DebugEntityMouseAreas", scrn, SKeys.F9,  KeyModifier.None, false);
			DebugSettings.AddSwitch("DBG", "ShowOperations",        scrn, SKeys.F10, KeyModifier.None, false);
			DebugSettings.AddSwitch("DBG", "DebugGestures",         scrn, SKeys.F11, KeyModifier.None, false);

			DebugSettings.AddPush("DBG",  "ShowDebugShortcuts",     scrn, SKeys.Tab,       KeyModifier.None);
			DebugSettings.AddPush("DBG",  "ShowSerializedProfile",  scrn, SKeys.O,         KeyModifier.None);
			DebugSettings.AddPush("DBG",  "AssimilateCannon",       scrn, SKeys.A,         KeyModifier.None);
			DebugSettings.AddPush("DBG",  "AbandonCannon",          scrn, SKeys.S,         KeyModifier.None);
			DebugSettings.AddPush("TRUE", "LeaveScreen",            scrn, SKeys.Backspace, KeyModifier.Control);
		}

#endif
	}
}
