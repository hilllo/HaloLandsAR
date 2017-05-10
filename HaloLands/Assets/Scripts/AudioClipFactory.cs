using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipFactory {

    private static AudioClip[] VOStagesClips;
    private static AudioClip[] VODynamicClips;

    public static List<AudioClip> GetVOClipsWithStage(string stage)
    {
        if (VOStagesClips == null || VOStagesClips.Length == 0)
        {
            VOStagesClips = Resources.LoadAll<AudioClip>("Audio/VO/Stages");
        }
        List<AudioClip> clips = new List<AudioClip>();
        if(stage == "Init")
        {
            FindClips(VOStagesClips, "Init", ref clips);
        }else if (stage == "Tutorial")
        {
            FindClips(VOStagesClips, "Tutorial", ref clips);
        }
        else if (stage == "First")
        {
            FindClips(VOStagesClips, "First", ref clips);
        }
        else if (stage == "Second")
        {
            FindClips(VOStagesClips, "Second", ref clips);
        }
        else if (stage == "Third")
        {
            FindClips(VOStagesClips, "Third", ref clips);
        }
        return clips;
    }

    public static List<AudioClip> GetVOClipsWithReactionType(ReactionType reaction)
    {
        if(VODynamicClips == null || VODynamicClips.Length == 0)
        {
            VODynamicClips = Resources.LoadAll<AudioClip>("Audio/VO/Fighting");
        }
        List<AudioClip> clips = new List<AudioClip>();
        switch (reaction)
        {
            case ReactionType.GhostActiveHint:
                FindClips(VODynamicClips, "Fighting01", ref clips);
                break;
            case ReactionType.GhostCloseHint:
                FindClips(VODynamicClips, "Fighting02", ref clips);
                break;
            case ReactionType.SpellKillNothingHint:
                FindClips(VODynamicClips, "Fighting03", ref clips);
                break;
            case ReactionType.SpellOutdatedHint:
                FindClips(VODynamicClips, "Fighting00", ref clips);
                break;
            case ReactionType.SpellToBatSpiderHint:
                FindClips(VODynamicClips, "Fighting06", ref clips);
                break;
            case ReactionType.GhostKillHint:
                FindClips(VODynamicClips, "Fighting04", ref clips);
                break;
            default:
                FindClips(VODynamicClips, "Fighting05", ref clips);
                break;
        }
        return clips;
    }

    static void FindClips(AudioClip[] clips, string containedString, ref List<AudioClip> result)
    {
        foreach(AudioClip clip in clips)
        {
            if (clip.name.Contains(containedString)){
                result.Add(clip);
            }
        }
    }
}
