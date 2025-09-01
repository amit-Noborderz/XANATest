using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BD
{
    public class NFTDataLoader : GenericSingleton<NFTDataLoader>
    {
        public string csvFileName = "NFTData.csv"; // Place your CSV file in Resources folder

        private Dictionary<string, NFTData> nftDataMap = new();

        void Start()
        {
            LoadCSVData();
        }

        void LoadCSVData()
        {
            TextAsset csvFile = Resources.Load<TextAsset>(csvFileName.Replace(".csv", ""));
            if (csvFile == null)
            {
                Debug.LogError("CSV file not found.");
                return;
            }

            StringReader reader = new(csvFile.text);

            // Read header line
            string headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
            {
                Debug.LogError("CSV file is empty or missing header row.");
                return;
            }

            string[] headers = ParseCSVLine(headerLine);
            Dictionary<string, int> headerIndexMap = new();

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndexMap[headers[i]] = i;
            }

            // Check if required columns exist
            if (!headerIndexMap.ContainsKey("Token ID") ||
                !headerIndexMap.ContainsKey("Profile") ||
                !headerIndexMap.ContainsKey("Speed"))
            {
                Debug.LogError("CSV file is missing required columns.");
                return;
            }

            // Read each line
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;

                string[] fields = ParseCSVLine(line);

                // Ensure we have enough fields
                if (fields.Length != headers.Length)
                {
                    Debug.LogWarning("Line has incorrect number of fields. Skipping line.");
                    continue;
                }

                NFTData data = new()
                {
                    // Parse Token ID
                    TokenID = fields[headerIndexMap["Token ID"]],

                    // Parse Profile (Character Name)
                    Profile = fields[headerIndexMap["Profile"]]
                };

                // Parse Damage Values
                string hpString = fields[headerIndexMap["Punch"]];
                if (!int.TryParse(hpString, out data.HP_Damage))
                {
                    Debug.LogWarning($"Invalid Punch value '{hpString}' for TokenID {data.TokenID}. Setting HP to 20.");
                    data.HP_Damage = 20;
                }
                
                string hkString = fields[headerIndexMap["Kick"]];
                if (!int.TryParse(hkString, out data.HK_Damage))
                {
                    Debug.LogWarning($"Invalid Kick value '{hkString}' for TokenID {data.TokenID}. Setting HK to 40.");
                    data.HK_Damage = 40;
                }
                
                //string hpString = fields[headerIndexMap["Heavy Punch"]];
                //if (!int.TryParse(hpString, out data.HP_Damage))
                //{
                    //Debug.LogWarning($"Invalid HP value '{hpString}' for TokenID {data.TokenID}. Setting HP to 200.");
                    //data.HP_Damage = 200;
                //}
                
                //string hkString = fields[headerIndexMap["Heavy Kick"]];
                //if (!int.TryParse(hkString, out data.HK_Damage))
                //{
                    //Debug.LogWarning($"Invalid HK value '{hkString}' for TokenID {data.TokenID}. Setting HK to 400.");
                    //data.HK_Damage = 400;
                //}
                
                string spString = fields[headerIndexMap["Special Move"]];
                if (!int.TryParse(spString, out data.SP_Damage))
                {
                    Debug.LogWarning($"Invalid SP value '{spString}' for TokenID {data.TokenID}. Setting SP to 500.");
                    data.SP_Damage = 500;
                }

                // Add to the map
                nftDataMap[data.TokenID] = data;
            }

            Debug.Log("CSV data loaded successfully.");
        }

        public NFTData GetNFTData(string tokenID)
        {
            if (nftDataMap.TryGetValue(tokenID, out NFTData data))
            {
                return data;
            }
            else
            {
                Debug.LogWarning($"TokenID {tokenID} not found in NFT data.");
                return null;
            }
        }

        // Basic CSV line parser that handles commas within quotes
        string[] ParseCSVLine(string line)
        {
            List<string> fields = new();
            bool inQuotes = false;
            string field = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(field);
                    field = "";
                }
                else
                {
                    field += c;
                }
            }
            fields.Add(field);

            return fields.ToArray();
        }
    }
}
