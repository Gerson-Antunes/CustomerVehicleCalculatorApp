using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CustomerVehicleCalculatorApp
{    
    public sealed partial class AppProject : Page
    {
         // Constant GST rate (10%)
         const double GST_RATE = 0.1;

         // Constants for optional extras prices
         const double WINDOW_TINTING_PRICE = 150;
         const double DUCO_PROTECTION_PRICE = 180;
         const double FLOOR_MATS_PRICE = 320;
         const double DELUXE_SOUND_PRICE = 350;

         // Constants for warranty price percentages
         const double WARRANTY_1_PERCENTAGE = 0;
         const double WARRANTY_2_PERCENTAGE = 0.05;
         const double WARRANTY_3_PERCENTAGE = 0.10;
         const double WARRANTY_5_PERCENTAGE = 0.20;
        
         // Constants for the insurance rates
         const double INSURANCE_RATE_UNDER_25 = 0.20;
         const double INSURANCE_RATE_OVER_25 = 0.10;


         // Arrays to store customer names and phone numbers
         string[] names = new string[10];
         string[] phoneNumbers = new string[10];

         // Array to store vehicle makes
         string[] vehicleMakes = new string[8];        
        
        
        public AppProject()
        {
            this.InitializeComponent();
            
            // Start with toggle off, radio buttons disabled and unchecked
            insuranceToggleSwitch.IsOn = false;
            under25RadioButton.IsEnabled = false;
            over25RadioButton.IsEnabled = false;
            
        }

        
        /* Handles Save button click
           Validates that name and phone fields are filled
           Disables the name and phone fields once valid */
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(customerNameTextBox.Text))
            {
                var dialogMessage = new MessageDialog("Oops! Looks like you forgot to enter the customer's name.\n Please fill it in before continuing.");
                await dialogMessage.ShowAsync();
                customerNameTextBox.Focus(FocusState.Programmatic);
                return;
            }
            if (string.IsNullOrEmpty(phoneTextBox.Text))
            {
                var dialogMessage = new MessageDialog("Hey! We need the customer's phone number before we can continue.");
                await dialogMessage.ShowAsync();
                phoneTextBox.Focus(FocusState.Programmatic);
                return;
            }

            // Disable name and phone inputs and focus on vehicle price
            customerNameTextBox.IsEnabled = false;
            phoneTextBox.IsEnabled = false;
            vehiclePriceTextBox.Focus(FocusState.Programmatic);
        }


        // Reset button clears all input and output fields, resets controls and re-enables name and phone
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all text fields
            customerNameTextBox.Text = "";
            phoneTextBox.Text = "";
            vehiclePriceTextBox.Text = "";
            tradeInTextBox.Text = "";
            subAmountTextBox.Text = "";
            gstAmountTextBox.Text = "";
            finalAmountTextBox.Text = "";
            insertVehicleMakeTextBox.Text = "";

            // Re-enable name and phone fields and focus on the customer name
            phoneTextBox.IsEnabled = true;
            customerNameTextBox.IsEnabled = true;
            customerNameTextBox.Focus(FocusState.Programmatic);

            // Reset optional extras checkboxes
            windowTintingCheckBox.IsChecked = false;
            ducoProtectionCheckBox.IsChecked = false;
            floorMatsCheckBox.IsChecked = false;
            deluxeSoundSystemCheckBox.IsChecked = false;

            // Set warranty to default (1 Year)
            warrantyComboBox.SelectedIndex = 0;

            // Reset insurance toggle and summary
            insuranceToggleSwitch.IsOn = false;
            summaryTextBlock.Text = string.Empty;
        }


        // Summary button validates, calculates and displays all costs
        private async void summaryButton_Click(object sender, RoutedEventArgs e)
        {
            double vehiclePrice, tradeIn;
            double subAmount, gstAmount, finalAmount;
            double vehicleWarranty, optionalExtras, accidentInsuranceCost;

            // If price fields are empty, treat them as zero
            if (string.IsNullOrWhiteSpace(vehiclePriceTextBox.Text))
            {
                vehiclePriceTextBox.Text = "0";
            }

            if (string.IsNullOrWhiteSpace(tradeInTextBox.Text))
            {
                tradeInTextBox.Text = "0";
            }

            // Try and Catch vehicle price
            try
            {
                vehiclePrice = double.Parse(vehiclePriceTextBox.Text);
            }
            catch (Exception ex)
            {
                var dialogMessage = new MessageDialog("Hmm... That doesn’t look like a valid vehicle price.\nPlease double-check and try again." + ex.Message);
                await dialogMessage.ShowAsync();
                vehiclePriceTextBox.Focus(FocusState.Programmatic);
                vehiclePriceTextBox.SelectAll();
                return;
            }

            // Try and Catch trade-in value
            try
            {
                tradeIn = double.Parse(tradeInTextBox.Text);
            }
            catch (Exception ex)
            {
                var dialogMessage = new MessageDialog("We couldn’t read the trade-in value.\nPlease enter a number (or leave it blank for zero)." + ex.Message);
                await dialogMessage.ShowAsync();
                tradeInTextBox.Focus(FocusState.Programmatic);
                tradeInTextBox.SelectAll();
                return;
            }

            //Input Validations
            if (vehiclePrice <= 0)
            {
                var dialogMessage = new MessageDialog("The vehicle price must be more than zero.\nPlease enter a valid amount.");
                await dialogMessage.ShowAsync();
                vehiclePriceTextBox.Focus(FocusState.Programmatic);
                vehiclePriceTextBox.SelectAll();
                return;
            }
            if (tradeIn < 0)
            {
                var dialogMessage = new MessageDialog("Trade-in value can’t be negative.\nIf there’s no trade-in, you can leave it at zero.");
                await dialogMessage.ShowAsync();
                tradeInTextBox.Focus(FocusState.Programmatic);
                tradeInTextBox.SelectAll();
                return;
            }
            if (vehiclePrice <= tradeIn)
            {
                var dialogMessage = new MessageDialog("The vehicle price must be higher than the trade-in value.\nPlease check your numbers.");
                await dialogMessage.ShowAsync();
                vehiclePriceTextBox.Focus(FocusState.Programmatic);
                vehiclePriceTextBox.SelectAll();
                return;
            }


            // Calculate warranty, extras, insurance            
            vehicleWarranty = calcVehicleWarranty(vehiclePrice);
            optionalExtras = calcOptionalExtras();
            accidentInsuranceCost = calcAccidentInsurance(vehiclePrice, optionalExtras);


            /* Perform calculations using double for currency
            Display results formatted as currency */
            subAmount = (vehiclePrice + vehicleWarranty + optionalExtras + accidentInsuranceCost - tradeIn);
            subAmountTextBox.Text = subAmount.ToString("C");

            // Calculate GST and final amount
            gstAmount = subAmount * GST_RATE;
            gstAmountTextBox.Text = gstAmount.ToString("C");

            finalAmount = subAmount + gstAmount;
            finalAmountTextBox.Text = finalAmount.ToString("C");

            // Display summary output
            summaryTextBlock.Text = "Summary Details:\n" +
               $"Customer Name: {customerNameTextBox.Text}\n" +
               $"Phone Number: {phoneTextBox.Text}\n\n" +
               $"Vehicle Cost: {vehiclePrice.ToString("C")}\n" +
               $"Trade-in Amount: -{tradeIn.ToString("C")}\n" +
               $"Warranty Cost: +{vehicleWarranty.ToString("C")}\n" +
               $"Optional Extras Cost: +{optionalExtras.ToString("C")}\n" +
               $"Insurance Cost: +{accidentInsuranceCost.ToString("C")}\n\n" +
               $"Final Amount: {finalAmount.ToString("C")}";
        }


        /// <summary>
        /// Calculates the warranty cost based on the selected warranty option.
        /// </summary>
        /// <param name="vehiclePrice">The base price of the vehicle.</param>
        /// <returns>A double value representing the warranty cost.</returns>
        private double calcVehicleWarranty(double vehiclePrice)
        {
            double vehicleWarranty = 0;

            if (warrantyComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectOption = selectedItem.Content.ToString();

                switch (selectOption)
                {
                    case "1 Year (No charge)":
                        vehicleWarranty = vehiclePrice * WARRANTY_1_PERCENTAGE;
                        break;
                    case "2 Years (5% of vehicle cost)":
                        vehicleWarranty = vehiclePrice * WARRANTY_2_PERCENTAGE;
                        break;
                    case "3 Years (10% of vehicle cost)":
                        vehicleWarranty = vehiclePrice * WARRANTY_3_PERCENTAGE;
                        break;
                    case "5 Years (20% of vehicle cost)":
                        vehicleWarranty = vehiclePrice * WARRANTY_5_PERCENTAGE;
                        break;
                    default:
                        vehicleWarranty = 0;
                        break;
                }
            }
            return vehicleWarranty;
        }


        /// <summary>
        /// Calculates the total cost of all selected optional extras.
        /// </summary>
        /// <returns>A double value representing the total optional extras cost.</returns>
        private double calcOptionalExtras()
        {
            double optionalExtras = 0;

            if (windowTintingCheckBox.IsChecked == true)
            {
                optionalExtras += WINDOW_TINTING_PRICE;
            }
            if (ducoProtectionCheckBox.IsChecked == true)
            {
                optionalExtras += DUCO_PROTECTION_PRICE;
            }
            if (floorMatsCheckBox.IsChecked == true)
            {
                optionalExtras += FLOOR_MATS_PRICE;
            }
            if (deluxeSoundSystemCheckBox.IsChecked == true)
            {
                optionalExtras += DELUXE_SOUND_PRICE;
            }
            return optionalExtras;

        }


        /// <summary>
        /// Calculates the accident insurance cost based on the customer's age group and selected extras.
        /// </summary>
        /// <param name="vehiclePrice">The price of the vehicle.</param>
        /// <param name="optionalExtras">The cost of the optional extras.</param>
        /// <returns>A double value representing the accident insurance cost.</returns>
        private double calcAccidentInsurance(double vehiclePrice, double optionalExtras)
        {
            double accidentInsurance = 0;

            if (insuranceToggleSwitch.IsOn)
            {
                double baseAmount = vehiclePrice + optionalExtras;

                if (under25RadioButton.IsChecked == true)
                {
                    accidentInsurance = baseAmount * INSURANCE_RATE_UNDER_25;
                }
                else if (over25RadioButton.IsChecked == true)
                {
                    accidentInsurance = baseAmount * INSURANCE_RATE_OVER_25;
                }
            }
            else
            {
                accidentInsurance = 0;
            }

            return accidentInsurance;
        }


        // Event handler for toggling the accident insurance switch
        private void insuranceToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (insuranceToggleSwitch.IsOn)
            {
                under25RadioButton.IsEnabled = true;
                over25RadioButton.IsEnabled = true;
                under25RadioButton.IsChecked = true;
            }
            else
            {
                under25RadioButton.IsEnabled = false;
                over25RadioButton.IsEnabled = false;
                under25RadioButton.IsChecked = false;
                over25RadioButton.IsChecked = false;
            }
        }



        //Populate arrays when the page loads
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate names array with sample data
            names[0] = "Amelia Brooks";
            names[1] = "Brandon Clark";
            names[2] = "Chloe Diaz";
            names[3] = "David Edwards";
            names[4] = "Emily Fisher";
            names[5] = "Gavin Howard";
            names[6] = "Hannah Jenkins";
            names[7] = "Isaac Morgan";
            names[8] = "Julia Parker";
            names[9] = "Kevin Scott";

            // Populate corresponding phone numbers
            phoneNumbers[0] = "9123 4567";
            phoneNumbers[1] = "9876 5432";
            phoneNumbers[2] = "3344 5566";
            phoneNumbers[3] = "8123 9876";
            phoneNumbers[4] = "9456 7890";
            phoneNumbers[5] = "9988 7766";
            phoneNumbers[6] = "3222 4455";
            phoneNumbers[7] = "8233 1122";
            phoneNumbers[8] = "9011 2233";
            phoneNumbers[9] = "9655 8877";

            // Populate vehicle makes
            vehicleMakes[0] = "Toyota";
            vehicleMakes[1] = "Holden";
            vehicleMakes[2] = "Mitsubishi";
            vehicleMakes[3] = "Ford";
            vehicleMakes[4] = "BMW";
            vehicleMakes[5] = "Mazda";
            vehicleMakes[6] = "Volkswagen";
            vehicleMakes[7] = "Mini";
        }

        //Display all customers in the summaryTextBlock
        private void displayAllCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            string output = "";

            // Loop through all customers
            for (int index = 0; index < phoneNumbers.Length; index ++)
            {
                // Append name and phone number to output string
                output = output + names[index] + " - " + phoneNumbers[index] + "\n";
            }

            // Show all customer details in the UI
            summaryTextBlock.Text = "All Customer Details:\n\n" + output;
        }

        //Search for a customer name
        private async void searchNameButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if the search textbox is empty
            if (customerNameTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Please enter a customer name to search.");
                await dialogMessage.ShowAsync();
                customerNameTextBox.Focus(FocusState.Programmatic);
                return;
            }    

            string criteria = customerNameTextBox.Text;

            //Call the display all customers event (as per requirement)
            displayAllCustomersButton_Click(sender, e);

            // Search for the name in the array
            int counter = searchName(criteria);

            if (counter == -1)
            {
                var dialogMessage = new MessageDialog(criteria + " was not found.");
                await dialogMessage.ShowAsync();
                              
            }
            else
            {
                // Name found, display corresponding phone number
                phoneTextBox.Text = phoneNumbers[counter];
            }
        }


        /// <summary>
        /// Performs a sequential search on the names array to find the index of the specified criteria.
        /// </summary>
        /// <param name="criteria">The name to search for.</param>
        /// <returns>The index of the matching name if found; otherwise, -1.</returns>
        private int searchName(string criteria)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].ToUpper() == criteria.ToUpper())
                {
                    return i;
                }
            } 
            return -1;
        }

        //Delete a customer name and phone number
        private async void deleteNameButton_Click(object sender, RoutedEventArgs e)
        {
            int counter = 0;
            bool found = false;

            if (customerNameTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Please enter a customer name to delete.");
                await dialogMessage.ShowAsync();
                customerNameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            string criteria = customerNameTextBox.Text.ToUpper();

            // Search for customer in names array
            while (!found && counter < names.Length)
            {
                if (criteria == names[counter].ToUpper())
                    found = true;
                else
                    counter++;
            }

            if (counter < names.Length)
            {
                // Store name and phone to confirm deletion later
                string deleteName = names[counter];
                string deletePhone = phoneNumbers[counter];

                for (int i = counter; i < names.Length - 1; i++)
                {
                    names[i] = names[i + 1];
                    phoneNumbers[i] = phoneNumbers[i + 1];
                }

                Array.Resize(ref names, names.Length - 1);
                Array.Resize(ref phoneNumbers, phoneNumbers.Length - 1);

                displayAllCustomersButton_Click(sender, e);

                var dialogMessage = new MessageDialog(criteria + " 's name and phone number have been deleted. The customer list now contains " + names.Length + " entries.");
                await dialogMessage.ShowAsync();
            }
            else
            {              
                var dialogMessage = new MessageDialog(criteria + " was not found in the customer list.");
                await dialogMessage.ShowAsync();
                customerNameTextBox.Focus(FocusState.Programmatic);
            }
        }

        //Display all vehicle makes sorted alphabetically
        private void displayAllMakesButton_Click(object sender, RoutedEventArgs e)
        {
            string output = "";

            Array.Sort(vehicleMakes);

            for (int index = 0; index < vehicleMakes.Length; index++)
            {
                output = output + vehicleMakes[index] + "\n";
            }
            summaryTextBlock.Text = "All Vehicles Makes:\n\n" + output;
        }

        //Binary search for a vehicle make
        private async void binarySearchMakeButton_Click(object sender, RoutedEventArgs e)
        {
            string criteria = insertVehicleMakeTextBox.Text;
          
            if (insertVehicleMakeTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Please enter a vehicle make to search for.");
                await dialogMessage.ShowAsync();
                insertVehicleMakeTextBox.Focus(FocusState.Programmatic);
                return;
            }

            // Sort array before searching
            Array.Sort(vehicleMakes);
            displayAllMakesButton_Click(sender, e);

            // Perform binary search
            int foundIndex = stringArrayBinarySearch(vehicleMakes, criteria);                                 

            if (foundIndex == -1)
            {
                var dialogMessage = new MessageDialog(criteria + " vehicle make was not found.");
                await dialogMessage.ShowAsync();
                insertVehicleMakeTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                var dialogMessage = new MessageDialog(criteria + " vehicle make was found at index " + foundIndex + ".");
                await dialogMessage.ShowAsync();                
            }  
        }


        /// <summary>
        /// Performs a binary search on a sorted string array to find the index of the specified item.
        /// </summary>
        /// <param name="data">The sorted array of strings to search.</param>
        /// <param name="item">The string item to find.</param>
        /// <returns>The index of the matching item if found; otherwise, -1.</returns>
        public int stringArrayBinarySearch(string[] data, string item)
        {
            int min = 0;
            int max = data.Length - 1;
            int mid;

            string searchItem = item.ToUpper();

            while (min <= max)
            {
                mid = (min + max) / 2;
                string midValue = data[mid].ToUpper();

                if (midValue == searchItem)
                    return mid;

                if (string.Compare(searchItem, midValue) > 0)
                    min = mid + 1;
                else
                    max = mid - 1;
            } 
            return -1;
        }

        //Insert a new vehicle make if not exists
        private async void insertMakeButton_Click(object sender, RoutedEventArgs e)
        {
            string newMake = insertVehicleMakeTextBox.Text;

            if (insertVehicleMakeTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Please enter a vehicle make.");
                await dialogMessage.ShowAsync();
                insertVehicleMakeTextBox.Focus(FocusState.Programmatic);
                return;
            }

            Array.Sort(vehicleMakes);
            int existingIndex = stringArrayBinarySearch(vehicleMakes, newMake);

            if (existingIndex != -1)
            {
                var dialogMessage = new MessageDialog(newMake + " already exists at index " + existingIndex + ".");
                await dialogMessage.ShowAsync();                
            }
            else
            {
                Array.Resize(ref vehicleMakes, vehicleMakes.Length + 1);
                vehicleMakes[vehicleMakes.Length - 1] = newMake;

                Array.Sort(vehicleMakes);
                displayAllMakesButton_Click(sender, e);

                var dialogMessage = new MessageDialog("Vehicle make added. The array now contains " + vehicleMakes.Length + " items.");
                await dialogMessage.ShowAsync();
            }
        }    
    }
}

