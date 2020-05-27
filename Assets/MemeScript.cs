using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemeScript : MonoBehaviour
{
    [SerializeField] float spinAcceleration;
    float spinSpeed;
    [SerializeField] float flySpeed;

    [SerializeField] private string[] whatToTellEm;
    private Queue<string> _whatToTellEm;

    LayerMask _playerLayers;
    GameObject _player;

    bool _goodbye;

    Vector3 spinnyRotation;
    Quaternion rotationQ;

    // Start is called before the first frame update
    void Start()
    {
        _player = GlobalState.state.Player.gameObject;
        _playerLayers |= 1 << _player.layer;

        _whatToTellEm = new Queue<string>();

        for (int i = 0; i < whatToTellEm.Length; i++)
        {
            _whatToTellEm.Enqueue(whatToTellEm[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_goodbye)
        {
            spinSpeed += spinAcceleration * Time.deltaTime;
            spinnyRotation.y += Time.deltaTime * spinSpeed;
            rotationQ.eulerAngles = spinnyRotation;
            transform.rotation = rotationQ;
            spinnyRotation = transform.rotation.eulerAngles;

            transform.Translate(Vector3.up * flySpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_playerLayers == (_playerLayers | 1 << other.gameObject.layer))
        {
            StartCoroutine(TellEm());
        }
    }

    private IEnumerator TellEm()
    {
        if (_whatToTellEm.Count > 0)
        {
            Debug.Log(_whatToTellEm.Dequeue());
            yield return new WaitForSeconds(2.5f);
            //yield return new WaitForSeconds(0.5f);
            StartCoroutine(TellEm());
        }
        else
        {
            _goodbye = true;
            StartCoroutine(Goodbye());
            yield return null;
        }
    }

    private IEnumerator Goodbye()
    {
        yield return new WaitForSeconds(6f);
        Destroy(this.gameObject);
    }
}
