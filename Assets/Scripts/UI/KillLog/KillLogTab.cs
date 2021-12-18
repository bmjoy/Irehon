using Irehon.Chat;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillLogTab : MonoBehaviour
{
    [SerializeField]
    private Text murderName;
    [SerializeField]
    private Text killedName;

    private void Start()
    {
        this.StartCoroutine(this.Dissapear());
    }

    private IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(5f);

        CanvasGroup group = this.GetComponent<CanvasGroup>();

        while (group.alpha > 0)
        {
            group.alpha -= .1f;
            yield return new WaitForSeconds(.1f);
        }

        Destroy(this.gameObject);
    }

    public async void Intialize(ulong murderId, ulong killedId)
    {
        this.murderName.text = await SteamDataLoader.GetNicknameAsync(murderId);

        this.killedName.text = await SteamDataLoader.GetNicknameAsync(killedId);
    }
}
