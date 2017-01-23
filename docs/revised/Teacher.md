EA4S Teacher System
===============

*Edits:*

<table>
  <tr>
    <td>13-01-2017</td>
    <td>Michele Pirovano</td>
  </tr>
</table>

The Teacher System represents the language teacher inside the application.
It is an Expert System that controls the learning progression of the player based on:
  * Player journey progression
  * Player learning performance
  * Expert configuration
  * Minigame support requirements
It is designed to be agnostic to the specific language and highly configurable in respect to mini-games.

The Teacher System can be found under the **EA4S.Teacher** namespace.

In this document, the **Teacher** is a shorthand for the Teacher System.
The person or group of persons that configure the Teacher is instead referred to 
 singularly as the **expert**.
 

### Elements

The Teacher is composed of several elements:

**EA4S.TeacherAI** works is the entry point to the teacher functions. Helpers and Engines can be accessed from the **AppManager.Instance.Teacher** instance.
 
 * Engines are used to implement the expert system for a specific role:
	* **DifficultySelectionAI** is in charge of selecting what difficulty to use for a given minigame.
	* **MiniGameSelectionAI** is in charge of selecting what minigames to play during a given playsession
	* **WordSelectionAI** is in charge of selecting what dictionary data a minigame should use.  
	* **LogAI** handles the logging of play data at runtime.
	
 * Helper classes make interaction with the underlying Database straightforward:
    * **ScoreHelper** provides methods for storing, retrieving, and updating score values related to learning data.
	* **JourneyHelper** provides methods for retrieving and comparing data progression data from the database.
	* **WordHelper** provides method for retrieving and comparing dictionary data.

### Engines

#### Difficulty Selection Engine

This Engine is in charge of selecting what difficulty to use for a given minigame.

Mini games can be configured to be more or less difficult for the player.
 The difficulty value is related only to.
  See #TODO LINK MINIGAME CREATION DOCS#
 
The difficulty value depends on:
 * The age of the player. The game will be more difficult for older players.
 * The current performance of the player for the given minigame. The game is more difficult the better the player gets.
 * The current journey position of the player. The game is more difficult at advanced stages.
 
The weights of the different variables can be configured in **ConfigAI**.

##### Code Flow

The Difficulty Selection Engine is accessed through **TeacherAI.GetCurrentDifficulty(MiniGameCode miniGameCode)**.
 This is called by the **MiniGame Launcher** beore loading a specific minigame
  and assigned to the minigame's Game Configuration class.
 
 
#### MiniGame Selection Engine

This Engine is in charge of selecting what minigame to use for a given play session.

The selection of minigames depends on:
 * Whether there is a fixed sequence (*PlaySessionDataOrder.Sequence*) or a random one (*PlaySessionDataOrder.Random*). This is defined by the expert.
 * In case of a fixed sequence, in what order to select the minigames.
 * In case of a random sequence, the choice depends on:
	* What minigames are available at all in the application, as read from the database's **Db.MiniGameData**.
	* What minigames are supported / favoured by the current learning block, as read from the database's **Db.LearningBlockData**.
	* Whether the game was played recently or not (favour less played minigames).
 
The weights of the different variables can be configured in **ConfigAI**.


##### Code Flow
 
The MiniGame Selection Engine is accessed whenever a new play session start,
 through **TeacherAI.SelectMiniGamesForPlaySession()**.
 This is called by **TeacherAI.InitialiseCurrentPlaySession()**,
  triggered by the **MiniMap** script in the *Map scene* when a new 
   play session is about to start.
   
 

#### Word Selection Engine

This Engine is in charge of selecting what dictionary data a minigame should use in a given play session.

 based on player progression, player performance, and the minigame's requirements

The selection of dictionary data is a two-stage process.

The first stage filters all learning data based on:
 * The selected minigame's learning rules and requirements (performed through a configured **QuestionBuilder**)
 * The current journey progression, as read from the journey data in the database (LearningBlocks and Stages)
 * Previously selected data for the same Question Builder.
 
The first stage is needed to make sure that all data to be selected matches
 the player's knowledge.
 
A second stage selects the learning data using weighted selection,
 with weights based on:
 * The learning score of the dictionary data. Lower scores will prompt a given data entry to appear more.
 * The time since the player last saw that dictionary data. Entries that have not appeared for a while are preferred.
 * The focus of the current learning block.
 
 * In case of a random sequence, the choice depends on:
	* What minigames are available at all in the application, as read from the database's **Db.MiniGameData**.
	* What minigames are supported / favoured by the current learning block, as read from the database's **Db.LearningBlockData**.
	* Whether the game was played recently or not (favour less played minigames).
 
The weights of the different variables can be configured in **ConfigAI**.


##### Code Flow
 
### Minigame selection support

The Teacher System is designed so that many minigames can be supported with various requirements.
A procedure is needed to match what the teacher deems necessary for the current learning progression and what a given minigame can support.

As a simple example, the teacher cannot select minigames  


 *) A journey progression / minigame matching is provided to the teacher. This is used by the teacher to select minigames for a given playsession and make sure that a minigame can support at least some of the dictionary content for the learning objectives.
 
 *) The Teacher is first configured.
 

For this purpose, the 

When a new mingame is created, a **QuestionBuilder** must be assigned and configured in its Configuration class.
This is performed through the **IGameConfiguration.SetupBuilder()** method. The method must return an *IQuestionBuilder** that defines
 how Qoestion Packs are generated for the minigame.
 
A *QuestionBuilder* defines rules and requirements that the teacher should follow based on the minigame capabilities.
This includes:
	* the amount of Question Packs that a minigame instance can support
	* the type of data that the minigame wants to work with (Letters, Words, etc.)
	* The relationship among the different data objects (random words, all letters in a words, etc.)
	
A **QuestionBuilder** generates lists of *QuestionPacks* through the *CreateAllQuestionPacks** method.


Each *QuestionPack* contains ....

Several QuestionBuilders have been created to support common rules:
 * **AlphabetQuestionBuilder** provides all (or part of ) the letter of the alphabet.
 * **CommonLettersInWordBuilder** provides a 
 * refer to the API documentation for a complete list of question builders.

Note however that a new QuestionBuilder can be created for specific minigames, if the need arises.
 
the Teacher System can be configured by specifying rules

#### Question Builder implementation

@TODO: DESCRIBE



#### Configuration

The teacher can be configured by editing constants in the **ConfigAI** static class.

### Refactoring Notes

 * Helpers should probably belong to the DB, and not to the teacher.
 * LogAI should be a Helper