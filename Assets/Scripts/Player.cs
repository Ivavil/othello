using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Tile[] board = new Tile[Constants.NumTiles];
    public Node parent;
    public List<Node> childList = new List<Node>();
    public int type;//Constants.MIN o Constants.MAX
    public double utility;
    public double alfa;
    public double beta;

    public Node(Tile[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            this.board[i] = new Tile();
            this.board[i].value = tiles[i].value;
        }

    }    

}

public class Player : MonoBehaviour
{
    public int turn;    
    private BoardManager boardManager;

    void Start()
    {
        boardManager = GameObject.FindGameObjectWithTag("BoardManager").GetComponent<BoardManager>();
    }
       
    /*
     * Entrada: Dado un tablero
     * Salida: Posición donde mueve  
     */
    public int SelectTile(Tile[] board)
    {        
             
        //Generamos el nodo raíz del árbol (MAX)
        Node root = new Node(board);
        root.type = Constants.MAX;

        //Generamos primer nivel de nodos hijos
        List<int> selectableTiles = boardManager.FindSelectableTiles(board, turn);

        foreach (int s in selectableTiles)
        {
            //Creo un nuevo nodo hijo con el tablero padre
            Node n = new Node(root.board);
            //Lo añadimos a la lista de nodos hijo
            root.childList.Add(n);
            //Enlazo con su padre
            n.parent = root;
            //En nivel 1, los hijos son MIN
            n.type = Constants.MIN;
            //Aplico un movimiento, generando un nuevo tablero con ese movimiento
            boardManager.Move(n.board, s, turn);
        }

        //Generamos segundo nivel de nodos hijos
        foreach (Node s in root.childList)
        {
            //Calculamos las casillas disponibles para cada uno de los diferentes tableros creados en los nodos hijos de nivel 1 (para el turno de las negras)
            List<int> selectableTiles2 = boardManager.FindSelectableTiles(s.board, -1*turn);
            foreach (int t in selectableTiles2){
                Node n = new Node(s.board);
                //Lo añadimos a la lista de nodos hijo
                s.childList.Add(n);
                //Enlazo con su padre
                n.parent = s;
                //En nivel 1, los hijos son MIN
                n.type = Constants.MAX;
                //Aplico un movimiento, generando un nuevo tablero con ese movimiento
                boardManager.Move(n.board, t, -1*turn);
                //si queremos imprimir el nodo generado (tablero hijo)
                boardManager.PrintBoard(n.board);
                //Calculamos el valor de la función de utilidad del nodo
                int piezasJugador = boardManager.CountPieces(n.board, -1 * turn);
                int piezasIA = boardManager.CountPieces(n.board, turn);
                List<int> selectableTiles3 = boardManager.FindSelectableTiles(n.board, turn);
                HashSet<int> swappableTiles = new HashSet<int>();
                foreach (int swap in selectableTiles3){
                    List<int> swappableTilesList = boardManager.FindSwappablePieces(n.board, swap, turn);
                    foreach (int swapElement in swappableTilesList){
                        swappableTiles.Add(swapElement);
                    }
                }
                n.utility = 3 * (piezasIA - piezasJugador) + selectableTiles3.Count + swappableTiles.Count;
                Debug.Log("Jugador: "+ piezasJugador + " IA: " + piezasIA + " utilidad: " + n.utility + " casilla: " + t + " casillas disponibles: " + selectableTiles3.Count + " fichas girables: " + swappableTiles.Count);
            }
        }

        //A continuación realizaremos el volcado del mejor valor de los nodos hijos en su nodo padre
        //Usaremos 2 variables para almacenar los valores que volcaremos en sus respectivos nodos padre
        double valorVolcado2 = 100;
        double valorVolcado1 = -100;
        foreach (Node s in root.childList){
            foreach (Node z in s.childList){
                //Elegimos el valor más pequeño de los nodos hijos de nivel 2 para volcarlo en el nodo padre de tipo MIN
                if(z.utility < valorVolcado2){
                    valorVolcado2 = z.utility;
                }
            }
            //Volcamos el valor en el nodo padre de nivel 1 con el mejor valor del hijo de nivel 2 y reiniciamos la variable que usamos para elegir el mejor valor
            s.utility = valorVolcado2;
            valorVolcado2 = 100;

            //Volcamos al nodo raiz el mejor valor de los hijos de nivel 1
            if(s.utility > valorVolcado1){
                s.parent.utility = s.utility;
                valorVolcado1 = s.utility;
            }
        }

        //Comprobamos cual es el mejor movimiento que debe realizar la IA comparando los valores de los tableros del padre con los del hijo que realizó el volcado
        //Sabemos cual de los hijos realizó el volcado porque la funcion de utilidad tiene el mismo valor que el padre, después, comparando los valores de las casillas,
        //aquella en la que en el padre tenía un 0 (vacía) y el hijo un 1 o -1, es la casilla en la que se debe colocar la nueva ficha
        foreach (Node s in root.childList){
            if(s.utility == s.parent.utility){
                for(int i = 0; i < Constants.NumTiles; i++){
                    if(s.parent.board[i].value == 0 && s.board[i].value != 0){
                        Debug.Log("La mejor jugada es poner la ficha en la casilla " + i);
                        return i;
                    }
                }
            }
        }
    }
}
