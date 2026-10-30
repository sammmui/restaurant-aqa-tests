Feature: Signing in

  Customers who register can sign in with their own credentials. Waiter accounts
  are provided to the test environment and cannot be created by signing up.

  Rule: A waiter can sign in with the correct password

    Background:
      Given the restaurant has a waiter account

    @api @smoke
    Scenario: A waiter signs in
      When the waiter signs in with the correct password
      Then the waiter is signed in

    @ui @smoke
    Scenario: A waiter signs in on the storefront
      When the waiter signs in with the correct password
      Then the waiter is signed in

  Rule: Wrong credentials are refused

    @api @negative
    Scenario: A waiter who gets the password wrong is refused
      Given the restaurant has a waiter account
      When the waiter signs in with a wrong password
      Then sign-in is refused
      And the visitor is told "Invalid email or password"

    # Known defect (expected to fail until fixed): someone who never registered gets a server
    # error instead of the same refusal a wrong password gets.
    @api @negative @known-defect
    Scenario: Someone who never registered is refused the same way
      When someone who never registered tries to sign in
      Then sign-in is refused

    @ui @negative @customer-auth
    Scenario: A registered customer who enters the wrong password is refused
      Given a visitor is on the sign-up page
      When a visitor signs up with valid details
      And the customer signs in with an incorrect password
      Then sign-in is refused
      And the login form shows "Incorrect email or password. Try again or create an account."

    @ui @negative @customer-auth
    Scenario: Someone with an unknown email is refused
      When someone who never registered tries to sign in
      Then sign-in is refused
      And the login form shows "Incorrect email or password. Try again or create an account."

  Rule: Invalid sign-in details are rejected with an explanation

    @api @negative @validation
    Scenario Outline: Someone cannot sign in with an invalid detail
      When someone tries to sign in with an invalid <field> (<problem>)
      Then the visitor is told "<message>"

      @email
      Examples: Email address problems
        | field         | problem           | message                                                                         |
        | email address | without an @ sign | Invalid email address. Please ensure it follows the format: username@domain.com |
        | email address | without a domain  | Invalid email address. Please ensure it follows the format: username@domain.com |
        | email address | empty             | Email must be a valid email address and cannot exceed 254 characters.           |
        | email address | too long          | Email must be a valid email address and cannot exceed 254 characters.           |

      @password
      Examples: Password problems
        | field    | problem | message                                                       |
        | password | empty   | Password is required. Please enter your password to continue. |

    @ui @negative @validation @customer-auth
    Scenario Outline: The login form explains a missing sign-in detail
      When someone tries to sign in with an invalid <field> (<problem>)
      Then the login form shows "<message>"

      @email
      Examples: Missing email address
        | field         | problem | message                                                          |
        | email address | empty   | Email address is required. Please enter your email to continue. |

      @password
      Examples: Missing password
        | field    | problem | message                                                       |
        | password | empty   | Password is required. Please enter your password to continue. |
