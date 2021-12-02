using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;

public class KillLogTab : MonoBehaviour
{
    [SerializeField]
    private Text murderName;
    [SerializeField]
    private Text killedName;

    private void Start()
    {
        StartCoroutine(Dissapear());
    }

    private IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(5f);

        CanvasGroup group = GetComponent<CanvasGroup>();

        while (group.alpha > 0)
        {
            group.alpha -= .1f;
            yield return new WaitForSeconds(.1f);
        }

        Destroy(gameObject);
    }

    public async void Intialize(ulong murderId, ulong killedId)
    {
        murderName.text = await SteamUserInformation.GetNicknameAsync(murderId);

        killedName.text = await SteamUserInformation.GetNicknameAsync(killedId);
    }
}
