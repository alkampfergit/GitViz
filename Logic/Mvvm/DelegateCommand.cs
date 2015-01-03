using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitViz.Logic.Mvvm
{
    public class DelegateCommand : ICommand
    {
        public static readonly DelegateCommand Empty = DelegateCommand.Create().OnCanExecute(o => false);

        public static DelegateCommand Create()
        {
            return new DelegateCommand(null, null, String.Empty);
        }

        public static DelegateCommand Create(String displayText)
        {
            return new DelegateCommand(null, null, displayText);
        }

        protected Action<Object> executeMethod = null;
        private Func<Object, Boolean> canExecuteMethod = null;
        public event EventHandler CanExecuteChanged;
        protected EventHandler triggerHanlder = null;

        /// <summary>
        /// Raises <seealso cref="CanExecuteChanged"/> on the UI thread.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        private InputBindingCollection inputBindings;

        /// <summary>
        /// Constructor. Initializes delegate command with Execute delegate and CanExecute delegate
        /// </summary>
        /// <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        /// <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command.  This can be null.</param>
        /// <param name="displayText">Text displayed by elements this command is bound to</param>
        public DelegateCommand(Action<Object> executeMethod, Func<Object, Boolean> canExecuteMethod, String displayText)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
            this.DisplayText = displayText;

            this.triggerHanlder = (s, e) => this.EvaluateCanExecute();
        }

        public DelegateCommand OnExecute(Action<Object> executeMethod)
        {
            this.executeMethod = executeMethod;
            return this;
        }

        public DelegateCommand OnCanExecute(Func<Object, Boolean> canExecuteMethod)
        {
            this.canExecuteMethod = canExecuteMethod;
            return this;
        }

        /// <summary>
        /// Adds a new gesture to associate inputbindings
        /// </summary>
        public DelegateCommand AddGesture(InputGesture gesture)
        {
            if (this.inputBindings == null)
            {
                this.inputBindings = new InputBindingCollection();
            }

            this.inputBindings.Add(new InputBinding(this, gesture));

            return this;
        }

        public DelegateCommand AddKeyGesture(Key key)
        {
            return this.AddKeyGesture(key, ModifierKeys.None);
        }

        public DelegateCommand AddKeyGesture(Key key, ModifierKeys mk)
        {
            if (this.inputBindings == null)
            {
                this.inputBindings = new InputBindingCollection();
            }

            this.inputBindings.Add(new InputBinding(this, new KeyGesture(key, mk)));

            return this;
        }

        /// <summary>
        /// Gets command display text
        /// </summary>
        public string DisplayText
        {
            get;
            private set;
        }

        /// <summary>
        /// Command's associated input bindings
        /// </summary>
        public InputBindingCollection InputBindings
        {
            get { return this.inputBindings; }
        }

        ///<summary>
        ///Defines the method that determines whether the command can execute in its current state.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        ///<returns>
        ///true if this command can be executed; otherwise, false.
        ///</returns>
        public bool CanExecute(object parameter)
        {
            if (canExecuteMethod == null)
            {
                return true;
            }

            return canExecuteMethod(parameter);
        }

        ///<summary>
        ///Defines the method to be called when the command is invoked.
        ///</summary>
        ///<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            if (executeMethod == null)
            {
                return;
            }

            //Il try-catch serve per evitare che l'esecuzione di un comando faccia esplodere il software, si può 
            //gentilmente loggare l'eccezione e buonanotte.
            try
            {
                executeMethod(parameter);
            }
            catch (Exception)
            {
                //App._managementService.Log(new LogDto() { Message = "Exception during execution of command", FullExceptionData = ex.ToString() });
            }


        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker can requery to check if the command can execute.
        /// <remarks>Note that this will trigger the execution of <see cref="CanExecute"/> once for each invoker.</remarks>
        /// </summary>
        public virtual void EvaluateCanExecute()
        {
            this.OnCanExecuteChanged();
        }


        public void ClearTriggers()
        {
            this.triggerHanlder = null;
        }


    }
}
