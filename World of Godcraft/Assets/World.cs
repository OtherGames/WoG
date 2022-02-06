using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World 
{
    internal ChunckComponent[,,] chuncks;

	public static readonly Vector3[] faceChecks = new Vector3[6] 
	{
		new Vector3( 0.0f, 0.0f,-1.0f),
		new Vector3( 0.0f, 0.0f, 1.0f),
		new Vector3( 0.0f, 1.0f, 0.0f),
		new Vector3( 0.0f,-1.0f, 0.0f),
		new Vector3(-1.0f, 0.0f, 0.0f),
		new Vector3( 1.0f, 0.0f, 0.0f),
	};

	internal ref ChunckComponent GetChunk(Vector3 globalPosBlock)
	{
		int xIdx = Mathf.FloorToInt(globalPosBlock.x / WorldGeneratorInit.size);
		int zIdx = Mathf.FloorToInt(globalPosBlock.z / WorldGeneratorInit.size);

		return ref chuncks[xIdx, 0, zIdx];
	}

	public World()
    {
		int size = WorldGeneratorInit.worldSize;

		chuncks = new ChunckComponent[size, size, size];

	}
}
