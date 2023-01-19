using UnityEngine;

namespace DAT.MeshExplosion
{
    /// <summary>
    /// 爆発オブジェクトの親にアタッチ。
    /// プレイヤーと接触したら子のオブジェクトの吹き飛ばしを呼ぶ
    /// </summary>
    public class ExplodeParent : MonoBehaviour
    {
        [SerializeField]
        DebrisParams debrisParams = default;

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Player"))
            {
                SetExplode();
            }
        }

        void SetExplode()
        {
            var col = GetComponent<BoxCollider>();
            col.enabled = false;

            var exps = GetComponentsInChildren<ExplodeDebris>();
            for (int i = 0; i < exps.Length; i++)
            {
                exps[i].Explode(debrisParams);
            }

            Destroy(gameObject, debrisParams.lifeTime);
        }
    }
}