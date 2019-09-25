# SteeringAI
Unity AI Self Driving Car

Span of Project: 12/6/18

Revisited: March 2019

**Simulation Background**
  This game simulation is an attempt to create a model that can traverse a racing car track.

**Where To Play**  
https://simmer.io/@apkirito/steeringai

**How To Play**
1. Press Play to play the game yourself.
2. Press Train to train a generation of cars.
3. Press Test to test a trained car.
4. Use arrow keys to speed up/slow down the simulation.

**Script Accomplishments**
 - used custom Matrix class, Neural Network class from FlappyBirdAI
 - implemented Genetic Algorithm (mutation, cross over, repopulate)
 - better rewarding system (gave more points for staying in the middle of the track)
 - trained within minutes to make a car that can traverse multiple tracks
 - created a ui for playing, training, and testing
 - wrote custom script that makes a car track using Unity's MenuItem 

**Notes**
 - used high mutation (15%)
 - only cross over weights that share the same neuron(rows only)
 - had 2 parents picked from a pool selection algorithm
 - parents made 2 babies instead of 1
 - save best brain as a JSON into a txt file
 - load brain into game