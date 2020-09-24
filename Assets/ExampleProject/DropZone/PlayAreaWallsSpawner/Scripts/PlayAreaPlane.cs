using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public enum EPlayAreaPlaneType
    {
        NONE = 0,
        WALL = 1,
        FLOOR = 2,
        CEILING = 3,
    }

    [RequireComponent(typeof(MeshFilter))]
    public class PlayAreaPlane : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods
        public EPlayAreaPlaneType PlayAreaWallType { get { return playAreaPlaneType; } set { playAreaPlaneType = value; } }

        public void SetVertices(Vector3[] vertices)
        {
            if (ARE_EQUAL(vertices.Length, 4))
            {
                // Create Triangles
                int[] triangles = new int[]
                {
                    0, 3, 2,
                    2, 1, 0,
                    0, 1, 2,
                    2, 3, 0
                };

                // Create UVs
                Vector2[] uvs = new Vector2[]
                {
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f)
                };

                // Set up mesh
                myMesh.Clear();
                myMesh.vertices = vertices;
                myMesh.triangles = triangles;
                myMesh.uv = uvs;

                // Add collision
                gameObject.AddComponent<MeshCollider>();
            }
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private EPlayAreaPlaneType playAreaPlaneType = EPlayAreaPlaneType.NONE;

        #endregion

        #region Private Variables
        private Mesh myMesh;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            myMesh = new Mesh();
            GetComponent<MeshFilter>().mesh = myMesh;
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
