
# IEHSoftLabsTakeHome

##### Disclamer:
As for now I have not managed to make `docker-compose` build this whole project. I am struggling with SQL server inside my docker containers, and I have been implementing that logic in the [docker-compose-implementation](https://github.com/Uraharadono/IEHSoftLabsTakeHome/tree/docker-compose-implementation) branch.

### Steps to run (with Visual studio - no docker):

1. Open project, package manage console, run command: `Update-Database`
2. Run docker app
3. Run rabbitMq (should run automatically on system startup tho)
4.  Navigate to the AnalysisWorker project directory
run command: `cd AnalysisWorker`
Then build the Docker image 
then run: `docker build --no-cache -t analysis-worker:latest .`
(*docker build -t analysis-worker:latest .*)

5. In Visual studio set following projects as startup ones: **FoodTester.Api** & **AnalysisEngine**
 AnalysisWorker project will be resolved dynamically by **AnalysisEngine**.

# How I designed flow:
![image](https://github.com/user-attachments/assets/8bdea940-00df-4438-99ec-4835058334e2)


# Info
1. Project with name "**QualityManager** " in pdf of task, I have named "**FoodTester.Api**". 
2. Also, `QualityManagerController` could be separated on `Batch` i `Analysis` controllers. Why didn't I rename them then? Because of the same reason I did not rename "**FoodTester.Api**" project. Because I think a lot of thing will get broken now that I have almost gotten to the end of the implementation.
3. **AnalysisWorker** has  `Protos` folder, same as `Infrastructure` project. I know they should not be duplicated, but my docker build is failing because of it, and I seriously lack time to figure out why and how to fix it.
4. `AnalysisResults` table has `0:1` relationship with `AnalysisRequests` table. There is no special reason for that really. I wanted to make this task more interesting for myself. This relation can be `1 to many` as well, maybe that even makes more sense.
5. 

