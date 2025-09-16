using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FakeLoading : MonoBehaviour
{
    public TMP_Text percentage;
    public Image fill;
    public float percent;
    public bool play;
    public bool done;
    public Animator anim;
    public enum Ttype { publish, save, other };
    public Ttype Loading;

    private void OnEnable()
    {
        if (Loading == Ttype.publish || Loading == Ttype.save)
        {
            play = true;
            Go();
        }
    }

    public void Go()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        if (!play)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            play = true;
        }
        else
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
            play = false;
        }

        if (percent< 100)
        {
            Go();
        }

    }

    public void Update()
    {
        if (play)
        {
            if (percent < 100)
            {
                percent += Time.deltaTime * 15;
            }
            else
            {
                percent = 100;
                play = false;
                StopCoroutine(Load());
                DoAfterLoad();
            }


            if (fill)
            {
                fill.fillAmount = percent / 100;
            }

            string per = Mathf.RoundToInt(percent).ToString() + "%";

            if (Loading != Ttype.other)
            {
                percentage.text = per;
            }
            else
            {
                percentage.text = "LOADING ASSETS... " + per;
            }

        }



    }

    public void StopPlay()
    {
        play = false;
    }

    public void DoAfterLoad()
    {
        if (!done)
        {
            if (Loading != Ttype.other)
            {
                StartCoroutine(Type());
                done = true;
            }
            else
            {
                anim.SetBool("Load", true);
            }
            
        }
    }

    IEnumerator Type()
    {
        while (percentage.text.Length > 0)
        {
            yield return new WaitForSeconds(0.05f);
            percentage.text = percentage.text.Remove(percentage.text.Length - 1);
        }

        yield return new WaitForSeconds(1);

        if (Loading != Ttype.other)
        {
            int i = 0;
            string s = "Published!";

            if (Loading == Ttype.save)
            {
                s = "Saved!";
            }

            percentage.fontSize -= 15;

            while (i < s.Length)
            {
                yield return new WaitForSeconds(0.05f);
                percentage.text += s[i];
                i++;
            }
        }

        yield return new WaitForSeconds(3);

        if (Loading == Ttype.publish)
        {
            FindObjectOfType<Publishing>().Publish();
        }

        gameObject.transform.parent.gameObject.SetActive(false);

    }


}
