using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace SliderPuzzle
{
    public class SliderPuzzlePage : ContentPage
    {
        //Properties
        private const int SIZE = 4;

        private AbsoluteLayout _absoluteLayout;
        private Dictionary<GridPosition, GridItem> _gridItems;

        //Constructor
        public SliderPuzzlePage()
        {

            _gridItems = new Dictionary<GridPosition, GridItem>();
            _absoluteLayout = new AbsoluteLayout
            {
                BackgroundColor = Color.Blue,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var counter = 1;
            for (var row = 0; row < SIZE; row++)
            {
                
                for(var col = 0; col < SIZE; col++)
                {
                    GridItem item;
                    if (counter == 16)
                    {
                        item = new GridItem(new GridPosition(row, col), "Empty");
                        // Set property for the image string to grabe the right image and show a soled puzzle
                        item.ImgPath = "16";
                    }
                    else
                    {
                        item = new GridItem(new GridPosition(row, col), counter.ToString());
                    }
                    var tapRecognizer = new TapGestureRecognizer();
                    tapRecognizer.Tapped += OnLabelTapped;
                    item.GestureRecognizers.Add(tapRecognizer);

                    _gridItems.Add(item.CurrentPosition, item);
                    _absoluteLayout.Children.Add(item);

                    counter++;
                }
            }
            Suffle();

            ContentView contentView = new ContentView
            {
                Content = _absoluteLayout
            };
            contentView.SizeChanged += OnContentViewSizeChanged;
            this.Padding = new Thickness(5, Device.OnPlatform(25, 5, 5), 5, 5);
            this.Content = contentView;
           
        }
        void OnContentViewSizeChanged(object sender, EventArgs args)
        {
            ContentView contentView = (ContentView)sender;
            double squareSize = Math.Min(contentView.Width, contentView.Height) / SIZE;

            for(var row = 0; row < SIZE; row++)
            {
                for(var col = 0; col < SIZE; col++)
                {
                    GridItem item = _gridItems[new GridPosition(row, col)];
                    Rectangle rect = new Rectangle(col * squareSize, row * squareSize, squareSize, squareSize);
                    AbsoluteLayout.SetLayoutBounds(item, rect);
                }
            }
        }

        void OnLabelTapped(object sender, EventArgs args)
        {
            GridItem item = (GridItem)sender;

            if(item.isEmptySpot() == true)
            {
                return;
            }

            //Adjust random move to account for edges
            var counter = 0;
            
            while (counter < 4)
            {
                GridPosition pos = null;
                if (counter == 0 && item.CurrentPosition.Row != 0)
                {
                    //Get Position of Square above current item
                    pos = new GridPosition(item.CurrentPosition.Row - 1, item.CurrentPosition.Column);
                }
                else if (counter == 1 && item.CurrentPosition.Column != SIZE - 1)
                {
                    //Get Position of Square right current item
                    pos = new GridPosition(item.CurrentPosition.Row , item.CurrentPosition.Column + 1);
                }
                else if (counter == 2 && item.CurrentPosition.Row != SIZE - 1)
                {
                    //Get Position of Square below current item
                    pos = new GridPosition(item.CurrentPosition.Row + 1, item.CurrentPosition.Column);
                }
                else if (counter == 3 && item.CurrentPosition.Column != 0)
                {
                    //Get Position of Square left current item
                    pos = new GridPosition(item.CurrentPosition.Row, item.CurrentPosition.Column - 1);
                }
                if (pos != null)
                {
                    GridItem swapWith = _gridItems[pos];
                    if (swapWith.isEmptySpot())
                    {
                        Swap(item, swapWith);
                        CheckSolved();
                        break;
                    }
                }
                counter = counter + 1;
            }

           
            OnContentViewSizeChanged(this.Content, null);
        }

        void Swap(GridItem item1, GridItem item2)
        {
            GridPosition temp = item1.CurrentPosition;
            item1.CurrentPosition = item2.CurrentPosition;
            item2.CurrentPosition = temp;

            //update the dictionary
            _gridItems[item1.CurrentPosition] = item1;
            _gridItems[item2.CurrentPosition] = item2;
            
            
        }
        public void CheckSolved()
        {
            bool puzzsolved = true;
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                   GridItem item = _gridItems[new GridPosition(row, col)];
                    if (!item.isPositionCorrect())
                    {
                        puzzsolved = false;
                        break;
                    }
                }
            }
            if (puzzsolved== true)
            {
                GridItem item = _gridItems[new GridPosition(3, 3)];
                item.ShowWinImg();
            }
        }
            void Suffle()
        {
            Random rand = new Random();
            for (var row = 0; row < SIZE; row++)
            {
                for(var col=0; col<SIZE; col++)
                {
                    GridItem item = _gridItems[new GridPosition(row, col)];

                    int swapRow = rand.Next(0, 4);
                    int swapCol = rand.Next(0, 4);
                    GridItem swapItem = _gridItems[new GridPosition(swapRow, swapCol)];

                    Swap(item, swapItem);
                }
            }
        }
       


    }

    internal class GridItem : Image
    {
        public GridPosition CurrentPosition
        {
            get;
            set;
        }
        private GridPosition _finalPosition;
        private Boolean _isEmptySpot;

        public string ImgPath
        {
            get; set;
        }
        public GridItem(GridPosition position, String text)
        {
            _finalPosition = position;
            CurrentPosition = _finalPosition;
            if (text.Equals ("Empty"))
            {
                _isEmptySpot = true;
            }
            else
            {
                _isEmptySpot = false;
            }
            //Text = text;
            //TextColor = Color.White;
            Source = ImageSource.FromResource(("SliderPuzzle.img") + (text) + (".jpeg"));
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        public Boolean isEmptySpot()
        {
            return _isEmptySpot;
        }
        public void ShowWinImg()
        {
            if (isEmptySpot())
            {
                Source = ImageSource.FromResource(("SliderPuzzle.img16.jpeg"));  
            }
        }
        public Boolean isPositionCorrect()
        {
            return _finalPosition.Equals(CurrentPosition);
        }
        

       }

    internal class GridPosition
    {
        public int Row
        {
            get; set;
        }
        public int Column
        {
            get; set;
        }
        public GridPosition(int row, int col)
        {
            Row = row;
            Column = col;
        }
        public override bool Equals(object obj)
        {
            GridPosition other = obj as GridPosition;

            if(other != null && this.Row == other.Row && this.Column == other.Column)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 17 * (23 + this.Row.GetHashCode()) * (23 + this.Column.GetHashCode());
        }
    }
}
