using System.Collections;
using UnityEngine;

namespace Match3_Evo
{

    public class CollectedTileRoot : MonoBehaviour
    {

        public GameObject collectedTile;

        void Start()
        {
            GM.boardMng.collectedTileRoot = transform;

            RefillWithUsedTiles();
        }

        void RefillWithUsedTiles()
        {
            foreach (Transform lvChild in transform)
                GameObject.Destroy(lvChild.gameObject);
                
            GameObject lvNewTile;
            for (int i = 0; i < GM.boardMng.gameParameters.tileVariantMax; i++)
            {
                lvNewTile = Instantiate(collectedTile, transform);
                lvNewTile.GetComponent<CollectedTile>().image.sprite = GM.boardMng.FieldDatas[i].basic;
            }
        }

    }

}