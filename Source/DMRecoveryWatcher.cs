using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMagic
{
    [KSPAddonImproved(KSPAddonImproved.Startup.SpaceCenter | KSPAddonImproved.Startup.TrackingStation | KSPAddonImproved.Startup.Flight, false)]
    internal class DMRecoveryWatcher: MonoBehaviour
    {
        private DMScienceScenario science = DMScienceScenario.SciScenario;

        private void Start()
        {
            print("[DM] Starting Recovery Watcher");
            GameEvents.onVesselRecovered.Add(ProtoRecoveryWatcher);
            GameEvents.OnVesselRecoveryRequested.Add(RecoveryWatcher);
        }

        private void OnDestroy()
        {
            print("[DM] Destroying Recovery Watcher");
            GameEvents.onVesselRecovered.Remove(ProtoRecoveryWatcher);
            GameEvents.OnVesselRecoveryRequested.Remove(RecoveryWatcher);
        }

        private void EventDebug(string gevent)
        {
            print("GameEvent " + gevent + " triggered");
        }

        private ConfigNode SubjectNode()
        {
            ConfigNode node = new ConfigNode();
            
            return node;
        }

        private void RecoveryWatcher(Vessel v)
        {
            print("[DM] Vessel Recovery Triggered");
            List<ScienceData> dataList = new List<ScienceData>();
            foreach (IScienceDataContainer container in v.FindPartModulesImplementing<IScienceDataContainer>())
            {
                dataList.AddRange(container.GetData());
            }
            if (dataList.Count > 0)
            {
                print("[DM] Found Data In Recovered Vessel");
                for (int i = 0; i < dataList.Count; i++)
                {
                    ScienceData data = dataList[i];
                    print("[DM] Searching For DMData...");


                    ScienceSubject sub = ResearchAndDevelopment.GetSubjectByID(data.subjectID);
                    sub.scientificValue = 0.5f;

                    if (DMScienceScenario.recoveredScienceList.Count > 0)
                    {
                        foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                        {
                            if (DMData.title == data.title)
                            {
                                float subVal = DMScienceScenario.SciScenario.SciSub(data.subjectID);
                                sub.scientificValue = DMData.scival;
                                //data.dataAmount = subVal * DMData.basevalue * DMData.scival * DMData.dataScale;
                                DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                            }
                        }
                    }
                }
                print("[DM] Setting Bool to True");
                DMScienceScenario.Recovered = true;
            }
        }

        private void ProtoRecoveryWatcher(ProtoVessel v)
        {
            print("[DM] ProtoVessel Recovery Triggered");
            EventDebug(v.vesselName);
            //ResearchAndDevelopment.Instance.Science = 10f;
            print(ResearchAndDevelopment.Instance.Science.ToString());
            
            if (!DMScienceScenario.Recovered)
            {
                foreach (ProtoPartSnapshot snap in v.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot msnap in snap.modules)
                    {
                        //foreach (ScienceData science in data.
                        foreach (ConfigNode dataNode in msnap.moduleValues.GetNodes("ScienceData"))
                        {
                            ScienceData data = new ScienceData(dataNode);
                            if (data != null)
                            {
                                print("[DM] Found Data In Recovered Vessel");
                                if (DMScienceScenario.recoveredScienceList.Count > 0)
                                {
                                    foreach (DMScienceScenario.DMScienceData DMData in DMScienceScenario.recoveredScienceList)
                                    {
                                        if (DMData.title == data.title)
                                        {
                                            float subVal = DMScienceScenario.SciScenario.SciSub(data.subjectID);
                                            data.dataAmount = subVal * DMData.basevalue * DMData.scival * DMData.dataScale;
                                            DMScienceScenario.SciScenario.submitDMScience(DMData, subVal);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                print("[DM] Data Already Recovered");
                DMScienceScenario.Recovered = false;
            }
        }

        

        

    }
}
