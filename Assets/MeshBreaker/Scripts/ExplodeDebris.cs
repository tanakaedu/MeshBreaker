using System.Collections;
using UnityEngine;

namespace DAT.MeshExplosion
{
    /// <summary>
    /// 爆発する破片オブジェクト
    /// メソッドが呼ばれたら渡された値に従ってランダムに速度と回転を設定
    /// </summary>
    public class ExplodeDebris : MonoBehaviour
    {
        static float BrinkSeconds => 1;

        public void Explode(DebrisParams debrisParams)
        {
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;

            var mesh = GetComponent<MeshFilter>();
            var normal = mesh.sharedMesh.normals[0];
            rb.velocity = Random.Range(debrisParams.speedMin, debrisParams.speedMax) * normal;
            rb.angularVelocity = new Vector3(
                Random.Range(-debrisParams.angularSpeed, debrisParams.angularSpeed),
                Random.Range(-debrisParams.angularSpeed, debrisParams.angularSpeed),
                0);

            var meshCol = GetComponent<MeshCollider>();
            if (meshCol)
            {
                meshCol.enabled = true;
            }

            StartCoroutine(Fade(debrisParams.lifeTime));
        }

        IEnumerator Fade(float sec)
        {
            var renderer = GetComponent<MeshRenderer>();
            for (float t = 0; t < sec; t += Time.deltaTime)
            {
                if (t >= sec - BrinkSeconds)
                {
                    renderer.enabled = (Time.frameCount & 1) == 0 ? true : false;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}