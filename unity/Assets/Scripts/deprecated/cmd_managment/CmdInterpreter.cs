using System.IO;
using UnityEngine;

public class CmdInterpreter : MonoBehaviour {

    //a StreamReader that will read the normalized log for further interpretation of the commands
    //private StreamReader logReaderNormalized = null;

	// Use this for initialization
	void Start () {

        //set a StreamReader on the normalized log file 
        //logReaderNormalized = GameObject.Find("Shared Variables").GetComponent<SharedVariables>().logReaderNormalized;
	}
	
	// Update is called once per frame
	void Update () {
	    /*ouvrir le fichier log.txt
        lire une ligne
        la garder en mémoire
		faire un while qui lit la ligne d'après tant qu'elle est identique à celle en mémoire
		quand on arrive à une ligne différente : on arrête, et on interpréte celle en mémoire
		cf site envoyé par le chercheur : au début seulement les commandes les plus fréquentes 
		il va falloir parser la ligne en mémoire pour voir à quelle commande possible elle correspond
		afficher le résultat dans la console, ou dans un autre fichier de log 
		attention : il faudra peut être écraser log.txt à chaque lancement de la scène, pour être sûr qu'on commence bien par calibrer le drone 
			=> les premières instructions doivent être 
			AT*FTRIM=1 à 10 
		Est-ce que c tjs de 1 à 10 ? Est-ce qu'une commande donnée de Jakopter correspond tjs exactement au même nombre de lignes envoyées à unity ? 
		Si oui : plus pratique !*/
	}
}
