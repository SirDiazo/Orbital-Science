﻿#region license
/* DMagic Orbital Science - Module Science Animate
 * Generic Part Module For Animated Science Experiments
 *
 * Copyright (c) 2014, David Grandy <david.grandy@gmail.com>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used 
 * to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMagic
{
	class DMModuleScienceAnimate: ModuleScienceExperiment, IScienceDataContainer
	{
		#region Fields

		[KSPField]
		public string customFailMessage = null;
		[KSPField]
		public string deployingMessage = null;
		[KSPField(isPersistant = true)]
		public bool IsDeployed;
		[KSPField]
		public string animationName = null;
		[KSPField]
		public string sampleAnim = null;
		[KSPField]
		public string indicatorAnim = null;
		[KSPField]
		public string sampleEmptyAnim = null;
		[KSPField]
		public float animSpeed = 1f;
		[KSPField]
		public string endEventGUIName = "Retract";
		[KSPField]
		public bool showEndEvent = true;
		[KSPField]
		public string startEventGUIName = "Deploy";
		[KSPField]
		public bool showStartEvent = true;
		[KSPField]
		public string toggleEventGUIName = "Toggle";
		[KSPField]
		public bool showToggleEvent = false;
		[KSPField]
		public bool showEditorEvents = true;

		[KSPField]
		public bool experimentAnimation = true;
		[KSPField]
		public bool experimentWaitForAnimation = false;
		[KSPField]
		public float waitForAnimationTime = -1;
		[KSPField]
		public int keepDeployedMode = 0;
		[KSPField]
		public bool oneWayAnimation = false;
		[KSPField]
		public string resourceExperiment = "ElectricCharge";
		[KSPField]
		public float resourceExpCost = 0;
		[KSPField]
		public bool asteroidReports = false;
		[KSPField]
		public bool asteroidTypeDependent = false;
		[KSPField(isPersistant = true)]
		public int experimentNumber = 0;
		[KSPField(isPersistant = true)]
		public int experimentsReturned = 0;
		[KSPField]
		public string storageFullMessage = "No more samples can be collected";
		[KSPField]
		public bool USScience = false;
		[KSPField]
		public bool USStock = false;
		[KSPField]
		public string bayAnimation = null;
		[KSPField]
		public string looperAnimation = null;
		[KSPField]
		public bool primary = true;
		[KSPField(isPersistant = true)]
		public int lastAsteroid = 0;
		[KSPField]
		public int sitMask = 0;
		[KSPField]
		public int bioMask = 0;
		[KSPField]
		public int experimentLimit = 1;
		[KSPField]
		public bool externalDeploy = false;

		protected Animation anim;
		private Animation anim2;
		private Animation anim3;
		private Animation anim4;
		private ScienceExperiment scienceExp;
		private bool resourceOn = false;
		private int dataIndex = 0;
		private List<ScienceData> scienceReports = new List<ScienceData>();
		private List<ScienceData> storedScienceReports = new List<ScienceData>();
		private List<DMEnviroSensor> enviroList = new List<DMEnviroSensor>();
		private List<DMModuleScienceAnimate> primaryList = new List<DMModuleScienceAnimate>();
		private DMModuleScienceAnimate primaryModule = null;
		private DMAsteroidScience newAsteroid;
		private const string bodyNameFixed = "Eeloo";
		private bool lastInOperableState = false;
		protected float scienceBoost = 1f;
		private int asteroidID = 0;
		private string failMessage = "";
		protected float labDataBoost = 0.5f;

		//You never know...
		public bool conduct
		{
			get {
			return canConduct();
			}
			private set {}
		}

		#endregion

		#region PartModule

		public override void OnStart(StartState state)
		{
			this.part.force_activate();
			if (!string.IsNullOrEmpty(animationName))
				anim = part.FindModelAnimators(animationName)[0];
			if (!string.IsNullOrEmpty(sampleAnim)) {
				anim2 = part.FindModelAnimators(sampleAnim)[0];
				secondaryAnimator(sampleAnim, 0f, experimentNumber * (1f / experimentLimit), 1f);
			}
			if (!string.IsNullOrEmpty(indicatorAnim)) {
				anim2 = part.FindModelAnimators(indicatorAnim)[0];
				secondaryAnimator(indicatorAnim, 0f, experimentNumber * (1f / experimentLimit), 1f);
			}
			if (!string.IsNullOrEmpty(sampleEmptyAnim))
				anim2 = part.FindModelAnimators(sampleEmptyAnim)[0];
			if (!string.IsNullOrEmpty(looperAnimation))
				anim3 = part.FindModelAnimators(looperAnimation)[0];
			if (!string.IsNullOrEmpty(bayAnimation))
				anim4 = part.FindModelAnimators(bayAnimation)[0];
			if (state == StartState.Editor) editorSetup();
			else {
				setup();
				if (IsDeployed) {
					primaryAnimator(1f, 1f, WrapMode.Default, animationName, anim);
					if (anim4!= null)
						primaryAnimator(1f, 1f, WrapMode.Default, bayAnimation, anim4);
					if (anim3 != null)
						primaryAnimator(2.5f * animSpeed, 0f, WrapMode.Loop, looperAnimation, anim3);
				}
			}
		}

		public override void OnSave(ConfigNode node)
		{
			node.RemoveNodes("ScienceData");
			foreach (ScienceData storedData in storedScienceReports) {
				ConfigNode storedDataNode = node.AddNode("ScienceData");
				storedData.Save(storedDataNode);
			}
		}

		public override void OnLoad(ConfigNode node)
		{
			if (node.HasNode("ScienceData")) {
				foreach (ConfigNode storedDataNode in node.GetNodes("ScienceData")) {
					ScienceData data = new ScienceData(storedDataNode);
					storedScienceReports.Add(data);
				}
			}
		}

		public override void OnUpdate()
		{
			if (resourceOn) {
				if (PartResourceLibrary.Instance.GetDefinition(resourceExperiment) != null) {
					float cost = resourceExpCost * Time.deltaTime;
					if (part.RequestResource(resourceExperiment, cost) < cost) {
						StopCoroutine("WaitForAnimation");
						resourceOn = false;
						ScreenMessages.PostScreenMessage("Not enough " + resourceExperiment + ", shutting down experiment", 4f, ScreenMessageStyle.UPPER_CENTER);
						if (keepDeployedMode == 0 || keepDeployedMode == 1) retractEvent();
					}
				}
			}
			//Durrrr, gameEvents sure are helpful....
			if (Inoperable)
				lastInOperableState = true;
			else if (lastInOperableState) {
				lastInOperableState = false;
				if (!string.IsNullOrEmpty(sampleEmptyAnim))
					secondaryAnimator(sampleEmptyAnim, animSpeed, 1f - (experimentNumber * (1f / experimentLimit)), experimentNumber * (anim2[sampleEmptyAnim].length / experimentLimit));
				else if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), experimentNumber * (anim2[sampleAnim].length / experimentLimit));
				if (!string.IsNullOrEmpty(indicatorAnim))
					secondaryAnimator(indicatorAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), experimentNumber * (anim2[indicatorAnim].length / experimentLimit));
				experimentNumber = 0;
				experimentsReturned = 0;
				if (keepDeployedMode == 0) retractEvent();
			}
			eventsCheck();
		}

		public override string GetInfo()
		{
			if (resourceExpCost > 0) {
				float time = waitForAnimationTime;
				if (time == -1 && anim != null && !string.IsNullOrEmpty(animationName))
					time = anim[animationName].length;
				string info = base.GetInfo();
				info += "Requires:\n-" + resourceExperiment + ": " + resourceExpCost.ToString() + "/s for " + waitForAnimationTime.ToString() + "s\n";
				return info;
			}
			else return base.GetInfo();
		}

		private void setup()
		{
			Events["deployEvent"].guiActive = showStartEvent;
			Events["retractEvent"].guiActive = showEndEvent;
			Events["toggleEvent"].guiActive = showToggleEvent;
			Events["deployEvent"].guiName = startEventGUIName;
			Events["retractEvent"].guiName = endEventGUIName;
			Events["toggleEvent"].guiName = toggleEventGUIName;
			Events["DeployExperiment"].guiName = experimentActionName;
			Events["DeployExperiment"].guiActiveUnfocused = externalDeploy;
			Events["DeployExperiment"].externalToEVAOnly = externalDeploy;
			Events["DeployExperiment"].unfocusedRange = interactionRange;
            Actions["deployAction"].guiName = startEventGUIName;
            Actions["retractAction"].guiName = endEventGUIName;
            Actions["toggleAction"].guiName = toggleEventGUIName;
            Actions["DeployAction"].guiName = experimentActionName;
			if (!primary) {
				primaryList = this.part.FindModulesImplementing<DMModuleScienceAnimate>();
				if (primaryList.Count > 0) {
					foreach (DMModuleScienceAnimate DMS in primaryList)
						if (DMS.primary) primaryModule = DMS;
				}
			}
			if (USStock)
				enviroList = this.part.FindModulesImplementing<DMEnviroSensor>();
			if (waitForAnimationTime == -1)
				waitForAnimationTime = anim[animationName].length / animSpeed;
			if (experimentID != null) {
				scienceExp = ResearchAndDevelopment.GetExperiment(experimentID);
				if (scienceExp != null && DMWhiteListMods.whiteListed) {
					scienceExp.situationMask = (uint)sitMask;
					scienceExp.biomeMask = (uint)bioMask;
				}
			}
			if (FlightGlobals.Bodies[16].bodyName != "Eeloo")
				FlightGlobals.Bodies[16].bodyName = bodyNameFixed;
			labDataBoost = xmitDataScalar / 2;
		}

		private void editorSetup()
		{
			Actions["deployAction"].active = showStartEvent;
			Actions["retractAction"].active = showEndEvent;
			Actions["toggleAction"].active = showToggleEvent;
			Actions["deployAction"].guiName = startEventGUIName;
			Actions["retractAction"].guiName = endEventGUIName;
			Actions["toggleAction"].guiName = toggleEventGUIName;
			Actions["ResetAction"].active = experimentLimit <= 1;
			Actions["DeployAction"].guiName = experimentActionName;
			Events["editorDeployEvent"].guiName = startEventGUIName;
			Events["editorRetractEvent"].guiName = endEventGUIName;
			Events["editorDeployEvent"].active = showEditorEvents;
			Events["editorRetractEvent"].active = false;
		}

		private void eventsCheck()
		{
			Events["ResetExperiment"].active = experimentLimit <= 1 && storedScienceReports.Count > 0;
			Events["ResetExperimentExternal"].active = storedScienceReports.Count > 0;
			Events["CollectDataExternalEvent"].active = storedScienceReports.Count > 0;
			Events["DeployExperiment"].active = !Inoperable;
			Events["DeployExperiment"].guiActiveUnfocused = !Inoperable && externalDeploy;
			Events["ReviewDataEvent"].active = storedScienceReports.Count > 0;
			Events["ReviewInitialData"].active = scienceReports.Count > 0;
		}

		#endregion

		#region Animators

		protected void primaryAnimator(float speed, float time, WrapMode wrap, string name, Animation a)
		{
			if (a != null) {
				a[name].speed = speed;
				if (!a.IsPlaying(name)) {
					a[name].wrapMode = wrap;
					a[name].normalizedTime = time;
					a.Blend(name, 1f);
				}
			}
		}

		private void secondaryAnimator(string whichAnim, float sampleSpeed, float sampleTime, float waitTime)
		{
			if (anim2 != null) {
				anim2[whichAnim].speed = sampleSpeed;
				anim2[whichAnim].normalizedTime = sampleTime;
				anim2.Blend(whichAnim, 1f);
				StartCoroutine(WaitForSampleAnimation(whichAnim, waitTime));
			}
		}

		private IEnumerator WaitForSampleAnimation(string whichAnimCo, float waitTimeCo)
		{
			yield return new WaitForSeconds(waitTimeCo);
			anim2[whichAnimCo].enabled = false;
		}

		[KSPEvent(guiActive = true, guiName = "Deploy", active = true)]
		public virtual void deployEvent()
		{
			primaryAnimator(animSpeed * 1f, 0f, WrapMode.Default, animationName, anim);
			IsDeployed = !oneWayAnimation;
			if (USScience) {
				if (anim4 != null) {
					primaryAnimator(animSpeed * 1f, 0f, WrapMode.Default, bayAnimation, anim4);
				}
				if (anim3 != null) {
					primaryAnimator(animSpeed * 2.5f, 0f, WrapMode.Loop, looperAnimation, anim3);
				}
			}
			if (USStock) {
				if (enviroList.Count > 0) {
					foreach (DMEnviroSensor DMES in enviroList) {
						if (!DMES.sensorActive && DMES.primary)
							DMES.toggleSensor();
					}
				}
			}
			Events["deployEvent"].active = oneWayAnimation;
			Events["retractEvent"].active = showEndEvent;
		}

		[KSPAction("Deploy")]
		public void deployAction(KSPActionParam param)
		{
			deployEvent();
		}

		[KSPEvent(guiActive = true, guiName = "Retract", active = false)]
		public virtual void retractEvent()
		{
			if (oneWayAnimation) return;
			primaryAnimator(-1f * animSpeed, 1f, WrapMode.Default, animationName, anim);
			IsDeployed = false;
			if (USScience) {
				if (anim4 != null) {
					if (anim[animationName].length > anim4[bayAnimation].length)
						primaryAnimator(-1f * animSpeed, (anim[animationName].length / anim4[bayAnimation].length), WrapMode.Default, bayAnimation, anim4);
					else
						primaryAnimator(-1f * animSpeed, 1f, WrapMode.Default, bayAnimation, anim4);
				}
				if (anim3 != null) {
					anim3[looperAnimation].normalizedTime = anim3[looperAnimation].normalizedTime % 1;
					anim3[looperAnimation].wrapMode = WrapMode.Clamp;
				}
			}
			if (USStock) {
				if (enviroList.Count > 0) {
					foreach (DMEnviroSensor DMES in enviroList) {
						if (DMES.sensorActive && DMES.primary) DMES.toggleSensor();
					}
				}
			}
			Events["deployEvent"].active = showStartEvent;
			Events["retractEvent"].active = false;
		}

		[KSPAction("Retract")]
		public void retractAction(KSPActionParam param)
		{
			retractEvent();
		}

		[KSPEvent(guiActive = true, guiName = "Toggle", active = true)]
		public void toggleEvent()
		{
			if (IsDeployed) retractEvent();
			else deployEvent();
		}

		[KSPAction("Toggle")]
		public void toggleAction(KSPActionParam Param)
		{
			toggleEvent();
		}

		[KSPEvent(guiActiveEditor = true, guiName = "Deploy", active = true)]
		public void editorDeployEvent()
		{
			deployEvent();
			IsDeployed = false;
			Events["editorDeployEvent"].active = oneWayAnimation;
			Events["editorRetractEvent"].active = !oneWayAnimation;
		}

		[KSPEvent(guiActiveEditor = true, guiName = "Retract", active = false)]
		public void editorRetractEvent()
		{
			retractEvent();
			Events["editorDeployEvent"].active = true;
			Events["editorRetractEvent"].active = false;
		}

		#endregion

		#region Science Events

		new public void ResetExperiment()
		{
			if (storedScienceReports.Count > 0) {
				if (experimentLimit > 1)
					ResetExperimentExternal();
				else {
					if (keepDeployedMode == 0) retractEvent();
					storedScienceReports.Clear();
				}
				lastAsteroid = 0;
			}
		}

		new public void ResetAction(KSPActionParam param)
		{
			ResetExperiment();
		}

		new public void ResetExperimentExternal()
		{
			if (storedScienceReports.Count > 0) {
				if (!string.IsNullOrEmpty(sampleEmptyAnim))
					secondaryAnimator(sampleEmptyAnim, animSpeed, 1f - (experimentNumber * (1f / experimentLimit)), experimentNumber * (anim2[sampleEmptyAnim].length / experimentLimit));
				else if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), experimentNumber * (anim2[sampleAnim].length / experimentLimit));
				if (!string.IsNullOrEmpty(indicatorAnim))
					secondaryAnimator(indicatorAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), experimentNumber * (anim2[indicatorAnim].length / experimentLimit));
				foreach (ScienceData data in storedScienceReports) {
					storedScienceReports.Remove(data);
					experimentNumber--;
				}
				if (experimentNumber < 0)
					experimentNumber = 0;
				if (keepDeployedMode == 0) retractEvent();
				lastAsteroid = 0;
			}
		}

		new public void CollectDataExternalEvent()
		{
			List<ModuleScienceContainer> EVACont = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceContainer>();
			if (storedScienceReports.Count > 0) {
				if (EVACont.First().StoreData(new List<IScienceDataContainer> { this }, false))
					foreach (ScienceData data in storedScienceReports)
						DumpData(data);
			}
		}

		#endregion

		#region Science Experiment Setup

		new public virtual void DeployExperiment()
		{
			if (canConduct()) {
				if (experimentAnimation) {
					if (anim.IsPlaying(animationName)) return;
					else {
						if (!primary) {
								if (!primaryModule.IsDeployed)
									primaryModule.deployEvent();
								IsDeployed = true;
							}
						if (!IsDeployed) {
							deployEvent();
							if (!string.IsNullOrEmpty(deployingMessage))
								ScreenMessages.PostScreenMessage(deployingMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
							if (experimentWaitForAnimation) {
								if (resourceExpCost > 0)
									resourceOn = true;
								StartCoroutine("WaitForAnimation", waitForAnimationTime);
							}
							else
								runExperiment();
						}
						else if (resourceExpCost > 0) {
							resourceOn = true;
							StartCoroutine("WaitForAnimation", waitForAnimationTime);
						}
						else runExperiment();
					}
				}
				else runExperiment();
			}
			else
				ScreenMessages.PostScreenMessage(failMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
		}

		new public void DeployAction(KSPActionParam param)
		{
			DeployExperiment();
		}

		private IEnumerator WaitForAnimation(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			resourceOn = false;
			runExperiment();
		}

		private void runExperiment()
		{
			ScienceData data = makeScience(scienceBoost);
			if (asteroidReports && (DMAsteroidScience.asteroidGrappled() || DMAsteroidScience.asteroidNear()))
				lastAsteroid = asteroidID;
			if (experimentLimit <= 1) {
				dataIndex = 0;
				storedScienceReports.Add(data);
				ReviewData();
			}
			else {
				scienceReports.Add(data);
				initialResultsPage();
			}
			if (keepDeployedMode == 1) retractEvent();
		}

		private float fixSubjectValue(ExperimentSituations s, float f, float boost)
		{
			float subV = f;
			if (s == ExperimentSituations.SrfLanded) subV = vessel.mainBody.scienceValues.LandedDataValue;
			else if (s == ExperimentSituations.SrfSplashed) subV = vessel.mainBody.scienceValues.SplashedDataValue;
			else if (s == ExperimentSituations.FlyingLow) subV = vessel.mainBody.scienceValues.FlyingLowDataValue;
			else if (s == ExperimentSituations.FlyingHigh) subV = vessel.mainBody.scienceValues.FlyingHighDataValue;
			else if (s == ExperimentSituations.InSpaceLow) subV = vessel.mainBody.scienceValues.InSpaceLowDataValue;
			else if (s == ExperimentSituations.InSpaceHigh) subV = vessel.mainBody.scienceValues.InSpaceHighDataValue;
			return subV * boost;
		}

		private string getBiome(ExperimentSituations s)
		{
			if ((bioMask & (int)s) == 0)
				return "";
			else {
				switch (vessel.landedAt) {
					case "LaunchPad":
						return vessel.landedAt;
					case "Runway":
						return vessel.landedAt;
					case "KSC":
						return vessel.landedAt;
					default:
						return FlightGlobals.currentMainBody.BiomeMap.GetAtt(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad).name;
				}
			}
		}

		private ExperimentSituations getSituation()
		{
			if (asteroidReports && DMAsteroidScience.asteroidGrappled())
				return ExperimentSituations.SrfLanded;
			if (asteroidReports && DMAsteroidScience.asteroidNear())
				return ExperimentSituations.InSpaceLow;
			switch (vessel.situation) {
				case Vessel.Situations.LANDED:
				case Vessel.Situations.PRELAUNCH:
					return ExperimentSituations.SrfLanded;
				case Vessel.Situations.SPLASHED:
					return ExperimentSituations.SrfSplashed;
				default:
					if (vessel.altitude < (vessel.mainBody.atmosphereScaleHeight * 1000 * Math.Log(1e6)) && vessel.mainBody.atmosphere) {
						if (vessel.altitude < vessel.mainBody.scienceValues.flyingAltitudeThreshold)
							return ExperimentSituations.FlyingLow;
						else
							return ExperimentSituations.FlyingHigh;
					}
					if (vessel.altitude < vessel.mainBody.scienceValues.spaceAltitudeThreshold)
						return ExperimentSituations.InSpaceLow;
					else
						return ExperimentSituations.InSpaceHigh;
			}
		}

		private string situationCleanup(ExperimentSituations expSit, string b)
		{
			if (asteroidReports && DMAsteroidScience.asteroidGrappled())
				return " from the surface of a " + b + " asteroid";
			if (asteroidReports && DMAsteroidScience.asteroidNear())
				return " while in space near a " + b + " asteroid";
			if (vessel.landedAt != "")
				return " from " + b;
			if (b == "") {
				switch (expSit) {
					case ExperimentSituations.SrfLanded:
						return " from  " + vessel.mainBody.theName + "'s surface";
					case ExperimentSituations.SrfSplashed:
						return " from " + vessel.mainBody.theName + "'s oceans";
					case ExperimentSituations.FlyingLow:
						return " while flying at " + vessel.mainBody.theName;
					case ExperimentSituations.FlyingHigh:
						return " from " + vessel.mainBody.theName + "'s upper atmosphere";
					case ExperimentSituations.InSpaceLow:
						return " while in space near " + vessel.mainBody.theName;
					default:
						return " while in space high over " + vessel.mainBody.theName;
				}
			}
			else {
				switch (expSit) {
					case ExperimentSituations.SrfLanded:
						return " from " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.SrfSplashed:
						return " from " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.FlyingLow:
						return " while flying over " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.FlyingHigh:
						return " from the upper atmosphere over " + vessel.mainBody.theName + "'s " + b;
					case ExperimentSituations.InSpaceLow:
						return " from space just above " + vessel.mainBody.theName + "'s " + b;
					default:
						return " while in space high over " + vessel.mainBody.theName + "'s " + b;
				}
			}
		}

		private bool canConduct()
		{
			failMessage = "";
			if (Inoperable) {
				failMessage = "Experiment is no longer functional; must be reset at a science lab or returned to Kerbin";
				return false;
			}
			else if ((experimentNumber >= experimentLimit) && experimentLimit > 1) {
				failMessage = storageFullMessage;
				return false;
			}
			else if (storedScienceReports.Count > 0 && experimentLimit <= 1) {
				failMessage = storageFullMessage;
				return false;
			}
			else if (asteroidReports && (DMAsteroidScience.asteroidGrappled() || DMAsteroidScience.asteroidNear())) {
				newAsteroid = new DMAsteroidScience();
				newAsteroid.body.bodyName = bodyNameFixed;
				if (newAsteroid.ID == lastAsteroid) {
					failMessage = "This asteroid has already been scanned";
					return false;
				}
			}
			if ((sitMask & (int)getSituation()) == 0) {
				failMessage = customFailMessage;
				return false;
			}
			else
				return true;
		}

		private ScienceData makeScience(float boost)
		{
			ExperimentSituations vesselSituation = getSituation();
			string biome = getBiome(vesselSituation);
			CelestialBody mainBody = vessel.mainBody;
			bool asteroids = false;

			//Check for asteroids and alter the biome and celestialbody values as necessary
			if (asteroidReports && (DMAsteroidScience.asteroidGrappled() || DMAsteroidScience.asteroidNear())) {
				newAsteroid = new DMAsteroidScience();
				asteroids = true;
				mainBody = newAsteroid.body;
				biome = "";
				if (asteroidTypeDependent) biome = newAsteroid.aClass;
			}

			ScienceData data = null;
			ScienceExperiment exp = ResearchAndDevelopment.GetExperiment(experimentID);
			ScienceSubject sub = ResearchAndDevelopment.GetExperimentSubject(exp, vesselSituation, mainBody, biome);
			sub.title = exp.experimentTitle + situationCleanup(vesselSituation, biome);

			if (asteroids) {
				asteroidID = newAsteroid.ID;
				sub.subjectValue = newAsteroid.sciMult;
				sub.scienceCap = exp.scienceCap * sub.subjectValue * 5;
				mainBody.bodyName = bodyNameFixed;
			}
			else {
				sub.subjectValue = fixSubjectValue(vesselSituation, sub.subjectValue, boost);
				sub.scienceCap = exp.scienceCap * sub.subjectValue;
			}

			if (sub != null)
				data = new ScienceData(exp.baseValue * sub.dataScale, xmitDataScalar, 0.5f, sub.id, sub.title);
			return data;
		}

		#endregion

		#region Results Pages

		private void newResultPage()
		{
			if (storedScienceReports.Count > 0) {
				ScienceData data = storedScienceReports[dataIndex];
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentLimit - 1)) && !rerunnable, transmitWarningText, true, data.labBoost < 1 && checkLabOps() && xmitDataScalar < 1, new Callback<ScienceData>(onDiscardData), new Callback<ScienceData>(onKeepData), new Callback<ScienceData>(onTransmitData), new Callback<ScienceData>(onSendToLab));
				ExperimentsResultDialog.DisplayResult(page);
			}
		}

		new public void ReviewData()
		{
			dataIndex = 0;
			foreach (ScienceData data in storedScienceReports) {
				newResultPage();
				dataIndex++;
			}
		}

		new public void ReviewDataEvent()
		{
			ReviewData();
		}

		[KSPEvent(guiActive = true, guiName = "Review Initial Data", active = false)]
		public void ReviewInitialData()
		{
			if (scienceReports.Count > 0)
				initialResultsPage();
		}

		private void initialResultsPage()
		{
			if (scienceReports.Count > 0) {
				ScienceData data = scienceReports[0];
				ExperimentResultDialogPage page = new ExperimentResultDialogPage(part, data, data.transmitValue, labDataBoost, (experimentsReturned >= (experimentLimit - 1)) && !rerunnable, transmitWarningText, true, data.labBoost < 1 && checkLabOps() && xmitDataScalar < 1, new Callback<ScienceData>(onDiscardInitialData), new Callback<ScienceData>(onKeepInitialData), new Callback<ScienceData>(onTransmitInitialData), new Callback<ScienceData>(onSendInitialToLab));
				ExperimentsResultDialog.DisplayResult(page);
			}
		}

		private void onDiscardData(ScienceData data)
		{
			if (storedScienceReports.Count > 0) {
				if (!string.IsNullOrEmpty(sampleEmptyAnim))
					secondaryAnimator(sampleEmptyAnim, animSpeed, 1f - (experimentNumber * (1f / experimentLimit)), anim2[sampleEmptyAnim].length / experimentLimit);
				else if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), anim2[sampleAnim].length / experimentLimit);
				if (!string.IsNullOrEmpty(indicatorAnim))
					secondaryAnimator(indicatorAnim, -1f * animSpeed, experimentNumber * (1f / experimentLimit), anim[indicatorAnim].length / experimentLimit);
				storedScienceReports.Remove(data);
				if (keepDeployedMode == 0) retractEvent();
				experimentNumber--;
				if (experimentNumber < 0)
					experimentNumber = 0;
				lastAsteroid = 0;
			}
		}

		private void onKeepData(ScienceData data)
		{
		}

		private void onTransmitData(ScienceData data)
		{
			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && storedScienceReports.Count > 0) {
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(new List<ScienceData> { data });
				DumpData(data);
			}
			else
				ScreenMessages.PostScreenMessage("No transmitters available on this vessel.", 4f, ScreenMessageStyle.UPPER_LEFT);
		}

		private void onSendToLab(ScienceData data)
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			if (checkLabOps() && storedScienceReports.Count > 0)
				labList.OrderBy(ScienceUtil.GetLabScore).First().StartCoroutine(labList.First().ProcessData(data, new Callback<ScienceData>(onComplete)));
			else
				ScreenMessages.PostScreenMessage("No operational lab modules on this vessel. Cannot analyze data.", 4f, ScreenMessageStyle.UPPER_CENTER);
		}

		protected virtual void onComplete(ScienceData data)
		{
			ReviewData();
		}

		private bool checkLabOps()
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			for (int i = 0; i < labList.Count; i++) {
				if (labList[i].IsOperational())
					return true;
			}
			return false;
		}

		private void onDiscardInitialData(ScienceData data)
		{
			if (scienceReports.Count > 0) {
				scienceReports.Remove(data);
				if (keepDeployedMode == 0) retractEvent();
			}
		}

		private void onKeepInitialData(ScienceData data)
		{
			if (experimentNumber >= experimentLimit) {
				ScreenMessages.PostScreenMessage(storageFullMessage, 5f, ScreenMessageStyle.UPPER_CENTER);
				initialResultsPage();
			}
			else if (scienceReports.Count > 0) {
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, animSpeed, experimentNumber * (1f / experimentLimit), anim2[sampleAnim].length / experimentLimit);
				if (!string.IsNullOrEmpty(indicatorAnim))
					secondaryAnimator(indicatorAnim, animSpeed, experimentNumber * (1f / experimentLimit), anim2[indicatorAnim].length / experimentLimit);
				storedScienceReports.Add(data);
				scienceReports.Remove(data);
				experimentNumber++;
			}
		}

		private void onTransmitInitialData(ScienceData data)
		{
			List<IScienceDataTransmitter> tranList = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
			if (tranList.Count > 0 && scienceReports.Count > 0) {
				if (!string.IsNullOrEmpty(sampleAnim))
					secondaryAnimator(sampleAnim, animSpeed, experimentNumber * (1f / experimentLimit), anim2[sampleAnim].length / experimentLimit);
				if (!string.IsNullOrEmpty(indicatorAnim))
					secondaryAnimator(indicatorAnim, animSpeed, experimentNumber * (1f / experimentLimit), anim2[indicatorAnim].length / experimentLimit);
				tranList.OrderBy(ScienceUtil.GetTransmitterScore).First().TransmitData(new List<ScienceData> { data });
				DumpInitialData(data);
				experimentNumber++;
			}
			else
				ScreenMessages.PostScreenMessage("No transmitters available on this vessel.", 4f, ScreenMessageStyle.UPPER_LEFT);
		}

		private void onSendInitialToLab(ScienceData data)
		{
			List<ModuleScienceLab> labList = vessel.FindPartModulesImplementing<ModuleScienceLab>();
			if (checkLabOps() && scienceReports.Count > 0)
				labList.OrderBy(ScienceUtil.GetLabScore).First().StartCoroutine(labList.First().ProcessData(data, new Callback<ScienceData>(onInitialComplete)));
			else
				ScreenMessages.PostScreenMessage("No operational lab modules on this vessel. Cannot analyze data.", 4f, ScreenMessageStyle.UPPER_CENTER);
		}

		protected virtual void onInitialComplete(ScienceData data)
		{
			initialResultsPage();
		}

		#endregion

		#region IScienceDataContainer

		ScienceData[] IScienceDataContainer.GetData()
		{
			return storedScienceReports.ToArray();
		}

		int IScienceDataContainer.GetScienceCount()
		{
			return storedScienceReports.Count;
		}

		bool IScienceDataContainer.IsRerunnable()
		{
			return IsRerunnable();
		}

		void IScienceDataContainer.ReviewData()
		{
			ReviewData();
		}

		void IScienceDataContainer.ReviewDataItem(ScienceData data)
		{
			ReviewData();
		}

		void IScienceDataContainer.DumpData(ScienceData data)
		{
			DumpData(data);
		}

		new private void DumpData(ScienceData data)
		{
			if (storedScienceReports.Count > 0) {
				experimentsReturned++;
				Inoperable = !IsRerunnable();
				storedScienceReports.Remove(data);
			}
		}

		private void DumpInitialData(ScienceData data)
		{
			if (scienceReports.Count > 0) {
				experimentsReturned++;
				Inoperable = !IsRerunnable();
				scienceReports.Remove(data);
			}
		}

		new private bool IsRerunnable()
		{
			if (rerunnable)
				return true;
			else
				return experimentsReturned < experimentLimit;
		}

		#endregion

	}
}
