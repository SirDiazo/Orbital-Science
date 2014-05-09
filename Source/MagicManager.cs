using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DMagic
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class MagicManager : MonoBehaviour
    {
        protected MagicDataTransmitter magicTransmitter;
        protected Vessel vessel; 

        public void Start()
        {
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onVesselDestroy.Add(OnVesselDestroyed);

            vessel = FlightGlobals.ActiveVessel;
            ScheduleRebuild();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselDestroy.Remove(OnVesselDestroyed);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onVesselChange.Remove(OnVesselChange);

            RemoveMagicTransmitter(false);
        }

        public bool IsBusy
        {
            get;
            private set;
        }

        public void ScheduleRebuild()
        {
            if (IsBusy)
            {
                try
                {
                    StopCoroutine("Rebuild");
                }
                catch { }
            }

            StartCoroutine("Rebuild");
        }

        public void OnVesselChange(Vessel v)
        {
            print("StorageCache.OnVesselChange");

            RemoveMagicTransmitter();
            vessel = v;
            ScheduleRebuild();
        }

        public void OnVesselModified(Vessel v)
        {
            print("StorageCache.OnVesselModified");

            if (vessel != v)
            {
                OnVesselChange(v);
            }
            else ScheduleRebuild();
        }

        public void OnVesselDestroyed(Vessel v)
        {
            if (vessel == v)
            {
                print("StorageCache.OnVesselDestroyed");

                //RemoveMagicTransmitter();
                // temporary: seeing if attempting to remove transmitter while
                // vessel is being destroyed is causing:
                //      Destroying object multiple times. Don't use DestroyImmediate on the same object in OnDisable or OnDestroy."
                magicTransmitter = null;
                vessel = null;
            }
        }

        private void RemoveMagicTransmitter(bool rootOnly = true)
        {
            if (vessel != null)
                if (vessel.rootPart != null)
                {
                    try
                    {
                        if (vessel.rootPart.Modules.Contains("MagicDataTransmitter"))
                            vessel.rootPart.RemoveModule(vessel.rootPart.Modules.OfType<MagicDataTransmitter>().Single());

                        if (!rootOnly)
                            foreach (var part in vessel.Parts)
                                if (part.Modules.Contains("MagicDataTransmitter"))
                                    part.RemoveModule(part.Modules.OfType<MagicDataTransmitter>().First());
                    }
                    catch (Exception e)
                    {
                        print(String.Format("RemoveMagicTransmitter: caught exception {0}", e));
                    }
                }

            magicTransmitter = null;
        }

        private System.Collections.IEnumerator Rebuild()
        {
            IsBusy = true;

            print("StorageCache: Rebuilding ...");

            if (FlightGlobals.ActiveVessel != vessel)
            {
                // this could be an indication that we're not monitoring
                // the active vessel properly
                print("StorageCache: Active vessel is not monitored vessel.");


                RemoveMagicTransmitter();
                vessel = FlightGlobals.ActiveVessel;
            }

            while (!FlightGlobals.ready || !vessel.loaded)
            {
                print("StorageCache.Rebuild - waiting");
                yield return new WaitForFixedUpdate();
            }

            // now we must deal with IScienceDataTransmitters, which are not 
            // quite as simple due to the MagicDataTransmitter we're faking

            // remove any existing magic transmitters
            //   note: we include non-root parts here because it's possible
            //         for two vessels to have merged, due to docking for instance.
            // 
            //          We could theoretically only remove MagicTransmitters from
            //          the root parts of docked vessels, but there may be cases
            //          where vessels that couldn't normally dock do (some kind of plugin perhaps)
            //          so I've opted for a general solution here
            RemoveMagicTransmitter(false);


            // count the number of "real" transmitters onboard
            List<IScienceDataTransmitter> transmitters = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();
            transmitters.RemoveAll(tx => tx is MagicDataTransmitter);


            if (transmitters.Count > 0)
            {
                // as long as at least one transmitter is "real", the
                // vessel's root part should have a MagicDataTransmitter
                if (transmitters.Any(tx => !(tx is MagicDataTransmitter)))
                {
                    magicTransmitter = vessel.rootPart.AddModule("MagicDataTransmitter") as MagicDataTransmitter;
                    print(String.Format("Added MagicDataTransmitter to root part {0}", FlightGlobals.ActiveVessel.rootPart.ConstructID));
                }
            }
            else
            {
                print(String.Format("Vessel {0} has no transmitters; no magic transmitter added", vessel.name));
            }

            IsBusy = false;
            print("Rebuilt StorageCache");
        }

    }
}
