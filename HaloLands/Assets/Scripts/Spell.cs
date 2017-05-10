using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : EyeDetector {

    [SerializeField]
    private GameObject[] letters;

    private string _text = "";
    private bool _hasBeenActivated = false;

    private GameObject _holder = null;

    public string text
    {
        get
        {
            return this._text;
        }
        set
        {
            if (this._text == value)
                return;

            this._text = value;
            this.UpdateContent();
        }
    }

    public bool hasBeenActivated
    {
        get
        {
            return this._hasBeenActivated;
        }
    }

    // Use this for initialization
    void Start()
    {
    }
	
	// Update is called once per frame
	void Update () {
        //Vector3 dir = (transform.position - Camera.main.transform.position).normalized;
        //transform.LookAt(transform.position + dir);
	}

    public void Activate(bool activate)
    {
        // TODO
        if (activate)
        {
            this._hasBeenActivated = true;
            SpellReveal();
            this.gameObject.SetActive(true); // TODO: Change this
        }
        else
        {
            this._hasBeenActivated = false;
            this.gameObject.SetActive(false); // TODO: Change this
            SpellHide();
        }
    }

    static float SPACE = 0.19f;

    private void UpdateContent()
    {
        if (_holder != null)
            Destroy(_holder);
        _text = _text.ToLower();
        _holder = new GameObject("spell_holder");
        _holder.transform.parent = transform;
        _holder.transform.localPosition = Vector3.zero;
        _holder.transform.localRotation = Quaternion.identity;
        float start = - (_text.Length - 1) * SPACE / 2f;
        char[] t = _text.ToCharArray();
        for (int i = 0; i < _text.Length; i++)
        {
            int index = t[i] - 'a';
            GameObject letter = (GameObject)Instantiate(letters[index]);
            letter.transform.parent = _holder.transform;
            letter.transform.localPosition = new Vector3(start + SPACE * i, 0, 0);
            letter.transform.localRotation = Quaternion.identity;
        }
    }

    private void SpellReveal()
    {
        if (_holder != null)
            _holder.SetActive(true);
        AudioManager.Instance.FadeTo(transform.GetComponent<AudioSource>(), 1f, 1f);
    }

    private void SpellHide()
    {
        if (_holder != null)
            _holder.SetActive(false);
        AudioManager.Instance.FadeTo(transform.GetComponent<AudioSource>(), 0f, 1f);
    }
}
