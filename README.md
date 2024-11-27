# IEHSoftLabsTakeHome

### Steps to run (`docker-compose`):

1. Open shell at root folder of the solution (use git one if you don't have any other)
2. Run following commands (in order):
- `docker-compose down` # Stop and remove all containers - isn't necessary for first run, but is if ran multiple times
- `docker-compose build` # Rebuild
- `docker-compose up --build` # Run with more verbose output
3. Navigate to: http://localhost:5000/index.html
4. Do stuff :)


### Steps to run (with Visual studio - no docker):
(***Disclaimer:*** *Creating the docker specific appsettings (appsettings.Docker.json) did not work, because whatever I did RabbitMq caused some issues. So, before resorting to running stuff in VisualStudio adjust the appsettings.json in FoodTester.Api and AnalysisEngine a.k.a ucomment commented lines, remove old ones.*)
1. Open project, package manage console, run command: `Update-Database`
2. Run docker app
3. Run rabbitMq (should run automatically on system startup tho)
4.  Navigate to the AnalysisWorker project directory
- _command_: `cd AnalysisWorker`
- Then build the Docker image 
- _command_: `docker build --no-cache -t analysis-worker:latest .`
(*docker build -t analysis-worker:latest .*)

5. In Visual studio set following projects as startup ones: **FoodTester.Api** & **AnalysisEngine**
** AnalysisWorker** project will be resolved dynamically by **AnalysisEngine**.

# How I designed flow:
![image](https://github.com/user-attachments/assets/8bdea940-00df-4438-99ec-4835058334e2)


# Info
1. Project with name "**QualityManager** " in pdf of task, I have named "**FoodTester.Api**". 
2. Also, `QualityManagerController` could be separated on `Batch` i `Analysis` controllers. Why didn't I rename them then? Because of the same reason I did not rename "**FoodTester.Api**" project. Because I think a lot of thing will get broken now that I have almost gotten to the end of the implementation, and I am really tired atm.
3. **AnalysisWorker** has  `Protos` folder, same as `Infrastructure` project. I know they should not be duplicated, but my docker build is failing because of it, and I seriously lack time to figure out why and how to fix it.
4. `AnalysisResults` table has `0:1` relationship with `AnalysisRequests` table. There is no special reason for that really. I wanted to make this task more interesting for myself. This relation can be `1 to many` as well, maybe that even makes more sense.
5. 

