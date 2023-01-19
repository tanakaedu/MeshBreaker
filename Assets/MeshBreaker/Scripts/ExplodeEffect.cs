using UnityEngine;

namespace DAT.MeshExplosion
{
    /// <summary>
    /// 破壊オブジェクトの親にアタッチするスクリプト。
    /// </summary>
    public class ExplodeEffect : MonoBehaviour
    {
        [Tooltip("発生させるエフェクト(パーティクル)")]
        public ParticleSystem particle;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Vector3 pos = transform.position;
                if (other.contacts.Length > 0)
                {
                    pos = other.contacts[0].point;
                }

                if (particle)
                {
                    var newParticle = Instantiate(particle, pos, Quaternion.identity);
                    newParticle.Play();
                }
            }
        }
    }
}