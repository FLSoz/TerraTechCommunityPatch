using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Payload.UI.Commands;
using Payload.UI.Commands.Steam;
using Steamworks;

namespace CommunityPatch.patches
{
    internal static class SubscribedModsPatch
    {
        internal const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        internal static FieldInfo m_SteamQuerySubscribedOp = typeof(ManMods).GetField("m_SteamQuerySubscribedOp", flags);
        internal static FieldInfo m_WaitingOnDownloads = typeof(ManMods).GetField("m_WaitingOnDownloads", flags);
        internal static FieldInfo m_WaitingOnWorkshopCheck = typeof(ManMods).GetField("m_WaitingOnWorkshopCheck", flags);
        internal static MethodInfo LoadWorkshopData = typeof(ManMods).GetMethod("LoadWorkshopData", flags);

        internal static void CheckForMoreSteamMods(uint page)
        {
            d.Log($"[CommunityPatch] Attempting to fetch page {page} of subscribed mods");
            ManMods manMods = Singleton.Manager<ManMods>.inst;
            m_WaitingOnWorkshopCheck.SetValue(manMods, false);
            SteamDownloadData nextData = SteamDownloadData.Create(SteamItemCategory.Mods, page);
            CommandOperation<SteamDownloadData> operation = (CommandOperation<SteamDownloadData>)m_SteamQuerySubscribedOp.GetValue(manMods);
            operation.Execute(nextData);
        }

        internal static bool Prefix(SteamDownloadData data)
        {
            // We're assuming Workshop is enabled if this has been called
            d.Log("[CommunityPatch] Received query resonse from Steam");
            if (data.HasAnyItems)
            {
                if (data.m_Items.Count >= Constants.kNumUGCResultsPerPage)
                {
                    ManMods manMods = Singleton.Manager<ManMods>.inst;

                    List<PublishedFileId_t> waitingOnDownloadList = (List<PublishedFileId_t>)m_WaitingOnDownloads.GetValue(manMods);
                    for (int i = 0; i < data.m_Items.Count; i++)
                    {
                        SteamDownloadItemData steamDownloadItemData = data.m_Items[i];
                        waitingOnDownloadList.Add(steamDownloadItemData.m_Details.m_nPublishedFileId);
                        LoadWorkshopData.Invoke(manMods, new object[] { steamDownloadItemData, false });
                    }

                    uint currPage = data.m_Page;
                    CheckForMoreSteamMods(currPage + 1);
                    return false;
                }
                d.Log($"[CommunityPatch] Found {data.m_Items.Count}, assuming there's no more");
            }
            else
            {
                d.Log("[CommunityPatch] NO mods found");
            }
            return true;
        }
    }
}
