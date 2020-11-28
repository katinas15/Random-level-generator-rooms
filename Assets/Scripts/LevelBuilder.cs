using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    public StartRoom startRoomPrefab;
    public EndRoom endRoomPrefab;
    public List<Room> spawnableRooms = new List<Room>();
    public Vector2 iterationRange = new Vector2(3,10);

    List<Doorway> availableDoorways = new List<Doorway>();

    StartRoom startRoom;
    EndRoom endRoom;
    List<Room> placedRooms = new List<Room>();

    LayerMask roomLayerMask;

    void Start(){
        roomLayerMask = LayerMask.GetMask("Room");
        StartCoroutine(GenerateLevel());
    }

    IEnumerator GenerateLevel(){
        WaitForSeconds startup = new WaitForSeconds(1);
        WaitForFixedUpdate interval = new WaitForFixedUpdate();

        yield return startup;

        //Place start room
        PlaceStartRoom();
        print("place start room");
        yield return interval;

        int iterations = Random.Range((int) iterationRange.x, (int) iterationRange.y);
        for(int i = 0; i < iterations; i++) {
            //place room from list
            PlaceCorridor();
            print("placed random room");
            yield return interval;
        }

        //place end room
        print("placed end room");
        PlaceEndRoom();
        yield return interval;


        yield return new WaitForSeconds(3);
    }

    void PlaceStartRoom(){
        startRoom = Instantiate(startRoomPrefab);
        startRoom.transform.parent = transform;
        AddDoorwaysToList(startRoom, availableDoorways);

        startRoom.transform.position = Vector3.zero;
        startRoom.transform.rotation = Quaternion.identity;

    }

    void AddDoorwaysToList(Room room, List<Doorway> list){
        foreach(Doorway doorway in room.doorways){
            int randomExitPoint = Random.Range(0, list.Count);
            list.Insert(randomExitPoint, doorway);
        }

    }

    void PlaceCorridor(){
        Room currentRoom = Instantiate(spawnableRooms[Random.Range(0, spawnableRooms.Count)]) as Room;
        currentRoom.transform.parent = transform;

        List<Doorway> allAvailableDoorways = new List<Doorway>(availableDoorways);
        List<Doorway> currentDoorways = new List<Doorway>();
        AddDoorwaysToList(currentRoom, currentDoorways);
        AddDoorwaysToList(currentRoom, availableDoorways);

        bool roomPlaced = false;

        foreach(Doorway availableDoorway in allAvailableDoorways){
            foreach(Doorway currentDoorway in currentDoorways){
                PositionRoomAtDoorway(ref currentRoom, currentDoorway, availableDoorway);

                if(CheckRoomOverlap(currentRoom)){
                    continue;
                }

                roomPlaced = true;
                placedRooms.Add(currentRoom);

                currentDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(currentDoorway);

                availableDoorway.gameObject.SetActive(false);
                availableDoorways.Remove(availableDoorway);

                break;
            }

            if(roomPlaced) break;
        }

        if(!roomPlaced){
            Destroy(currentRoom.gameObject);
            // ResetLevelGenerator();
        }
    }

    void PositionRoomAtDoorway(ref Room room, Doorway roomDoorway, Doorway targetDoorway){
        room.transform.position = Vector3.zero;
        room.transform.rotation = Quaternion.identity;

        Vector3 targetDoorwayEuler = targetDoorway.transform.eulerAngles;
        Vector3 roomDoorwayEuler = roomDoorway.transform.eulerAngles;

        float deltaAngle = Mathf.DeltaAngle(roomDoorwayEuler.y, roomDoorwayEuler.y);
        Quaternion currentRoomTargetRotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);
        room.transform.rotation = currentRoomTargetRotation * Quaternion.Euler(0, 100f, 0);

        Vector3 roomPositionOffset = roomDoorway.transform.position - room.transform.position;
        room.transform.position = targetDoorway.transform.position - roomPositionOffset;


    }

    bool CheckRoomOverlap(Room room){
        Bounds bounds = room.RoomBounds;
        bounds.Expand (-0.1f);

        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.size / 2, room.transform.rotation, roomLayerMask);

        if(colliders.Length > 0) {
            foreach(Collider c in colliders){
                if(c.transform.parent.gameObject.Equals(room.gameObject)){
                    continue;
                } else {
                    // overlapped
                    return true;
                }
            }
        }

        return false;
    }

    void PlaceEndRoom(){

    }

    void ResetLevelGenerator(){
        if(startRoom){
            Destroy(startRoom.gameObject);
        }

        if(endRoom){
            Destroy(endRoom.gameObject);
        }

        foreach(Room room in placedRooms){
            Destroy(room.gameObject);
        }

        placedRooms.Clear();
        availableDoorways.Clear();
    }
}
