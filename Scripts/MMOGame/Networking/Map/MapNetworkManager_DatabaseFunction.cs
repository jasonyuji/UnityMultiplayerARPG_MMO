using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        private async Task LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                ReadPartyJob job = new ReadPartyJob(Database, id);
                job.Start();
                await job.WaitFor();
                if (job.result != null)
                    parties[id] = job.result;
                else
                    parties.Remove(id);
                loadingPartyIds.Remove(id);
            }
        }

        private async Task LoadGuildRoutine(int id)
        {
            if (id > 0 && !loadingGuildIds.Contains(id))
            {
                loadingGuildIds.Add(id);
                ReadGuildJob job = new ReadGuildJob(Database, id, CurrentGameInstance.SocialSystemSetting.GuildMemberRoles);
                job.Start();
                await job.WaitFor();
                if (job.result != null)
                    guilds[id] = job.result;
                else
                    guilds.Remove(id);
                loadingGuildIds.Remove(id);
            }
        }

        private async Task SaveCharacterRoutine(IPlayerCharacterData playerCharacterData)
        {
            if (playerCharacterData != null && !savingCharacters.Contains(playerCharacterData.Id))
            {
                savingCharacters.Add(playerCharacterData.Id);
                UpdateCharacterJob job = new UpdateCharacterJob(Database, playerCharacterData);
                job.Start();
                await job.WaitFor();
                savingCharacters.Remove(playerCharacterData.Id);
                if (LogInfo)
                    Debug.Log("Character [" + playerCharacterData.Id + "] Saved");
            }
        }

        private async void SaveCharactersRoutine()
        {
            if (savingCharacters.Count == 0)
            {
                int i = 0;
                List<Task> tasks = new List<Task>();
                foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
                {
                    if (playerCharacter == null) continue;
                    tasks.Add(SaveCharacterRoutine(playerCharacter.CloneTo(new PlayerCharacterData())));
                    ++i;
                }
                await Task.WhenAll(tasks);
                if (LogInfo)
                    Debug.Log("Saved " + i + " character(s)");
            }
        }

        private async Task SaveBuildingRoutine(IBuildingSaveData buildingSaveData)
        {
            if (buildingSaveData != null && !savingBuildings.Contains(buildingSaveData.Id))
            {
                savingBuildings.Add(buildingSaveData.Id);
                UpdateBuildingJob job = new UpdateBuildingJob(Database, Assets.onlineScene.SceneName, buildingSaveData);
                job.Start();
                await job.WaitFor();
                savingBuildings.Remove(buildingSaveData.Id);
                if (LogInfo)
                    Debug.Log("Building [" + buildingSaveData.Id + "] Saved");
            }
        }

        private async void SaveBuildingsRoutine()
        {
            if (savingBuildings.Count == 0)
            {
                int i = 0;
                List<Task> tasks = new List<Task>();
                foreach (BuildingEntity buildingEntity in buildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    tasks.Add(SaveBuildingRoutine(buildingEntity.CloneTo(new BuildingSaveData())));
                    ++i;
                }
                await Task.WhenAll(tasks);
                if (LogInfo)
                    Debug.Log("Saved " + i + " building(s)");
            }
        }

        public override BuildingEntity CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            if (!initialize)
                new CreateBuildingJob(Database, Assets.onlineScene.SceneName, saveData).Start();
            return base.CreateBuildingEntity(saveData, initialize);
        }

        public override void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            new DeleteBuildingJob(Database, Assets.onlineScene.SceneName, id).Start();
        }
    }
}
