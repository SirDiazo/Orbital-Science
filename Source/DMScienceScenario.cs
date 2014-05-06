/* DMagic Orbital Science - DM Science Scenario
 * Scenario Module to store science results
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
 *  
 */


using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace DMagic
{
    public class DMScienceScenario: ScenarioModule
    {

        public static DMScienceScenario SciScenario
        {
            get
            {
                Game g = HighLogic.CurrentGame;
                foreach (ProtoScenarioModule pMod in g.scenarios)
                {
                    if (pMod.moduleName == typeof(DMScienceScenario).Name) return (DMScienceScenario)pMod.moduleRef;
                }
                return (DMScienceScenario)g.AddProtoScenarioModule(typeof(DMScienceScenario), GameScenes.FLIGHT).moduleRef;
            }
            set {}
        }

        internal static List<DMScienceData> recoveredScienceList = new List<DMScienceData>();

        public override void OnSave(ConfigNode node)
        {
            ConfigNode results_node = new ConfigNode("Asteroid Science");
            results_node.ClearNodes();
            foreach (DMScienceData data in recoveredScienceList)
            {
                ConfigNode scienceResults_node = new ConfigNode("Science");
                scienceResults_node.AddValue("id", data.id);
                scienceResults_node.AddValue("title", data.title);
                scienceResults_node.AddValue("dsc", data.dataScale);
                scienceResults_node.AddValue("scv", data.scival);
                scienceResults_node.AddValue("sbv", data.subval);
                scienceResults_node.AddValue("sci", data.science);
                scienceResults_node.AddValue("cap", data.cap);
                scienceResults_node.AddValue("expNo", data.expNo);
                
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            recoveredScienceList.Clear();
            ConfigNode results_node = node.GetNode("Asteroid Science");
            if (results_node != null)
            {
                foreach (ConfigNode scienceResults_node in results_node.GetNodes("Science"))
                {
                    string id = scienceResults_node.GetValue("id");
                    string title = scienceResults_node.GetValue("title");
                    float dsc = Convert.ToSingle(scienceResults_node.GetValue("dsc"));
                    float scv = Convert.ToSingle(scienceResults_node.GetValue("scv"));
                    float sbv = Convert.ToSingle(scienceResults_node.GetValue("sbv"));
                    float sci = Convert.ToSingle(scienceResults_node.GetValue("sci"));
                    float cap = Convert.ToSingle(scienceResults_node.GetValue("cap"));
                    int eNo = Convert.ToInt32(scienceResults_node.GetValue("expNo"));
                    RecordNewScience(id, title, dsc, scv, sbv, sci, cap, eNo);
                }
            }
        }
        
        internal class DMScienceData
        {
            internal string id, title;
            internal int expNo;
            internal float dataScale, scival, subval, science, cap;
        }

        internal void RecordNewScience(string id, string title, float dsc, float scv, float sbv, float sci, float cap, int eNo)
        {
            DMScienceData DMData = new DMScienceData();
            DMData.id = id;
            DMData.title = title;
            DMData.dataScale = dsc;
            DMData.scival = scv;
            DMData.subval = sbv;
            DMData.science = sci;
            DMData.cap = cap;
            DMData.expNo = eNo;
            recoveredScienceList.Add(DMData);
        }

        internal void AppendNewScience(string id, float scv, float sci, int eNo)
        {
            foreach (DMScienceData DMData in recoveredScienceList)
            {
                if (DMData.id == id)
                {
                    DMData.scival = scv;
                    DMData.science = sci;
                    DMData.expNo = eNo;
                    break;
                }
            }
        }

        internal void RemoveDMScience(DMScienceData DMdata)
        {
            recoveredScienceList.Remove(DMdata);
        }

    }
}
