/* DMagic Orbital Science - Asteroid Science
 * Class to setup asteroid science data
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
using System.Linq;
using UnityEngine;

namespace DMagic
{
    internal class DMAsteroidScience
    {
        internal string aClass = null;
        internal string aType = null;
        internal float sciMult = 0f;
        internal int aSeed = 0;
        private static Vessel asteroidVessel;
        internal CelestialBody body;

        internal DMAsteroidScience()
        {
            body = FlightGlobals.Bodies[16];
            asteroidVariables();
        }

        //Alter some of the values to give us asteroid specific results based on asteroid class, type, and current situation
        private void asteroidVariables()
        {
            if (asteroidNear())
            {
                ModuleAsteroid asteroidM = asteroidVessel.FindPartModulesImplementing<ModuleAsteroid>().First();
                aSeed = Math.Abs(asteroidM.seed);
                aClass = asteroidClass(asteroidM.prefabBaseURL);
                aType = asteroidSpectral(aSeed);
                sciMult = asteroidValue(aClass);
            }
            else if (asteroidGrappled())
            {
                ModuleAsteroid asteroidM = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().First();
                aSeed = Math.Abs(asteroidM.seed);
                aClass = asteroidClass(asteroidM.prefabBaseURL);
                aType = asteroidSpectral(aSeed);
                sciMult = asteroidValue(aClass) * 1.5f;
            }
        }
        
        //Less dumb method for getting the asteroid class, take the last character from ModuleAsteroid.prefabBaseURL
        private string asteroidClass(string s)
        {
            switch (s[s.Length-1])
            {
                case 'A':
                    return "Class A";
                case 'B':
                    return "Class B";
                case 'C':
                    return "Class C";
                case 'D':
                    return "Class D";
                case 'E':
                    return "Class E";
                default:
                    return "Class Unholy";
            }
        }

        private float asteroidValue(string aclass)
        {
            switch (aclass)
            {
                case "Class A":
                    return 1.5f;
                case "Class B":
                    return 3f;
                case "Class C":
                    return 5f;
                case "Class D":
                    return 8f;
                case "Class E":
                    return 10f;
                case "Class Unholy":
                    return 30f;
                default:
                    return 1f;
            }
        }

        //Assign a spectral type based on the ModuleAsteroid.seed value
        private string asteroidSpectral(int seed)
        {
            if (seed >= 10000000 && seed < 50000000) return "_C-Type";
            else if (seed >= 50000000 && seed < 70000000) return "_S-Type";
            else if (seed >= 70000000 && seed < 82500000) return "_M-Type";
            else if (seed >= 82500000 && seed < 87500000) return "_E-Type";
            else if (seed >= 87500000 && seed < 90000000) return "_P-Type";
            else if (seed >= 90000000 && seed < 92500000) return "_B-Type";
            else if (seed >= 92500000 && seed < 95000000) return "_A-Type";
            else if (seed >= 95000000 && seed < 97500000) return "_R-Type";
            else if (seed >= 97500000 && seed < 100000000) return "_G-Type";
            else return "Unknown Type";
        }

        //Are we attached to the asteroid, check if an asteroid part is on our vessel
        internal static bool asteroidGrappled()
        {
            if (FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleAsteroid>().Count > 0) return true;
            return false;
        }
        
        //Are we near the asteroid, cycle through existing vessels, only target asteroids within 2km
        internal static bool asteroidNear()
        {
            List<Vessel> vesselList = FlightGlobals.fetch.vessels;
            foreach (Vessel v in vesselList)
            {
                if (v != FlightGlobals.ActiveVessel)
                {
                    if (v.FindPartModulesImplementing<ModuleAsteroid>().Count > 0)
                    {
                        Part asteroidPart = v.FindPartModulesImplementing<ModuleAsteroid>().First().part;
                        Vector3 asteroidPosition = asteroidPart.transform.position;
                        Vector3 vesselPosition = FlightGlobals.ActiveVessel.transform.position;
                        double distance = (asteroidPosition - vesselPosition).magnitude;
                        if (distance < 2000)
                        {
                            asteroidVessel = v;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


    }
}
