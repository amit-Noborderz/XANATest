using BD;
using System.Collections.Generic;
using UnityEngine;

namespace BD
{
    public class ProfileSelector : GenericSingletonPersistent<ProfileSelector>
    {
        public string profile;

        private int _currentProfile = -1;
        public int CurrentProfile
        {
            get
            {
                if (_currentProfile == -1)
                {
                    _currentProfile = PlayerPrefs.GetInt("CurrentProfile", -1);
                    
                    if (_currentProfile == -1)
                    {
                        for (int i = 0; i < profiles.Count; i++)
                        {
                            if (NFTManager.HasCharacter(profiles[i]))
                            {
                                _currentProfile = i;
                                break;
                            }
                        }

                        if (_currentProfile == -1)
                        {
                            Debug.LogError("Player does not have any character to play. Using a random character for testing...");
                            _currentProfile = 0;
                        }

                    }
                }

                return _currentProfile;
            }
            set
            {
                PlayerPrefs.SetInt("CurrentProfile", value);
                PlayerPrefs.Save();
                _currentProfile = value;
            }
        }

        private int _opponentProfile = -1;
        internal int OpponentProfile
        {
            get
            {
                if (_opponentProfile == -1)
                {
                    Debug.LogError("Opponent profile is not set.");
                    _opponentProfile = FightingGameManager.instance.id2;
                }
                return _opponentProfile;
            }
            set
            {
                _opponentProfile = value;
            }
        }

        // internal string CurrentProfileName
        // {
        //     get
        //     {
        //         try
        //         {
        //             return profiles[CurrentProfile];
        //         }
        //         catch (Exception e)
        //         {
        //             Debug.LogError(e);
        //             return "-";
        //         }
        //     }
        // }

        public List<string> profiles = new List<string>();

        //Riken for mapping profile
        public void SetProfileFromName()
        {
            CurrentProfile = profiles.FindIndex(o => o == profile);
        }

    }
}