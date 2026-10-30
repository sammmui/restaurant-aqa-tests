Feature: Registration

  A visitor becomes a customer by signing up. Everyone who signs up is a customer;
  waiters and admins are never created this way. Each scenario uses a brand-new
  email address and a valid password unless it says one detail is invalid.

  Rule: A visitor can become a customer

    @api @smoke
    Scenario: A visitor registers and can then sign in
      When a visitor registers as Anna Tester with a new email address and a valid password
      Then the visitor is registered
      And the visitor can sign in with those details

  Rule: Invalid details are rejected with an explanation

    @api @negative @validation
    Scenario Outline: A visitor cannot register with an invalid detail
      When a visitor tries to register with an invalid <field> (<problem>)
      Then the visitor is told "<message>"

      @names
      Examples: First and last name rules
        | field      | problem            | message                                                              |
        | first name | empty              | First name must be between 2 and 50 characters long.                 |
        | first name | with digits        | First name can only contain Latin letters, hyphens, and apostrophes. |
        | last name  | with an underscore | Last name can only contain Latin letters, hyphens, and apostrophes.  |
        | first name | with a symbol      | First name can only contain Latin letters, hyphens, and apostrophes. |
        | last name  | with digits        | Last name can only contain Latin letters, hyphens, and apostrophes.  |
        | last name  | with a symbol      | Last name can only contain Latin letters, hyphens, and apostrophes.  |
        | first name | with a non-Latin letter | First name can only contain Latin letters, hyphens, and apostrophes. |
        | last name  | with a non-Latin letter | Last name can only contain Latin letters, hyphens, and apostrophes.  |

      @email
      Examples: Email address rules
        | field         | problem           | message                                                                         |
        | email address | without an @ sign | Invalid email address. Please ensure it follows the format: username@domain.com |
        | email address | without a domain  | Invalid email address. Please ensure it follows the format: username@domain.com |

      @password
      Examples: Password rules
        | field                 | problem                     | message                                            |
        | password confirmation | different from the password | Passwords do not match.                            |

  Rule: The sign-up form guides a visitor to a working account

    Background:
      Given a visitor is on the sign-up page

    @ui @validation
    Scenario: A visitor with valid details can submit the form
      When the visitor fills in the sign-up form with valid details
      Then the visitor can submit the form

    @ui @negative @validation
    Scenario Outline: A visitor cannot submit the form with an invalid detail
      When the visitor fills in the sign-up form with valid details
      And the visitor enters an invalid <field> (<problem>)
      Then the visitor cannot submit the form

      @names
      Examples: Name fields
        | field      | problem            |
        | first name | empty              |
        | first name | with digits        |
        | last name  | empty              |
        | last name  | with an underscore |
        | first name | with a symbol      |
        | last name  | with digits        |
        | last name  | with a symbol      |
        | first name | with a non-Latin letter |
        | last name  | with a non-Latin letter |

      @email
      Examples: Email address field
        | field         | problem           |
        | email address | empty             |
        | email address | without an @ sign |

      @password
      Examples: Password fields
        | field                 | problem                     |
        | password confirmation | different from the password |

    @ui @negative @validation
    Scenario Outline: The form explains what is wrong with a detail
      When the visitor fills in the sign-up form with valid details
      And the visitor enters an invalid <field> (<problem>)
      Then the visitor is told "<message>"

      @names
      Examples: First name messages
        | field      | problem     | message                                                 |
        | first name | empty       | First name is required.                                 |
        | first name | too long    | First name must be up to 50 characters.                 |
        | first name | with digits | Only Latin letters, hyphens, and apostrophes are allowed. |

      # Known defect (expected to fail until fixed): the last name field shows the first name's
      # wording ("First name is required.") for its required and length messages.
      @names @known-defect
      Examples: Last name messages
        | field     | problem            | message                                                   |
        | last name | empty              | Last name is required.                                    |
        | last name | too long           | Last name must be up to 50 characters.                    |
        | last name | with an underscore | Only Latin letters, hyphens, and apostrophes are allowed. |

      @email
      Examples: Email address messages
        | field         | problem           | message                                                                         |
        | email address | empty             | Email is required.                                                              |
        | email address | without an @ sign | Invalid email address. Please ensure it follows the format: username@domain.com |

    @ui @smoke
    Scenario: A newly signed-up customer can sign in afterwards
      When a visitor signs up with valid details
      And the visitor signs in with those details
      Then the customer is signed in

  Rule: Registration accepts values at their documented boundaries

    @api @validation
    Scenario Outline: A visitor can register with a value at an accepted length boundary
      When a visitor registers with an accepted <field> at the <boundary>
      Then the visitor is registered

      @boundary
      Examples: Accepted length boundaries
        | field         | boundary        |
        | first name    | minimum length  |
        | first name    | maximum length  |
        | last name     | minimum length  |
        | last name     | maximum length  |
        | email address | maximum length  |
        | password      | minimum length  |
        | password      | maximum length  |

    @ui @validation
    Scenario Outline: A visitor can submit a value at an accepted length boundary
      Given a visitor is on the sign-up page
      When the visitor fills in the sign-up form with valid details
      And the visitor enters an accepted <field> at the <boundary>
      Then the visitor can submit the form

      @boundary
      Examples: Accepted form boundaries
        | field         | boundary        |
        | first name    | minimum length  |
        | first name    | maximum length  |
        | last name     | minimum length  |
        | last name     | maximum length  |
        | email address | maximum length  |
        | password      | minimum length  |
        | password      | maximum length  |

    @api @negative @validation
    Scenario Outline: A visitor cannot register with a value beyond an accepted length boundary
      When a visitor tries to register with an invalid <field> (<problem>)
      Then the visitor is told "<message>"

      @boundary
      Examples: Rejected length boundaries
        | field         | problem   | message                                                                         |
        | first name    | too short | First name must be between 2 and 50 characters long.                            |
        | first name    | too long  | First name must be between 2 and 50 characters long.                            |
        | last name     | too short | Last name must be between 2 and 50 characters long.                             |
        | last name     | too long  | Last name must be between 2 and 50 characters long.                             |
        | email address | too long  | Email must be a valid email address and cannot exceed 254 characters.           |
        | password      | too short | Password must be between 8 and 16 characters long.                             |
        | password      | too long  | Password must be between 8 and 16 characters long.                             |

    @ui @negative @validation
    Scenario Outline: A visitor cannot submit a value beyond an accepted length boundary
      Given a visitor is on the sign-up page
      When the visitor fills in the sign-up form with valid details
      And the visitor enters an invalid <field> (<problem>)
      Then the visitor cannot submit the form

      @boundary
      Examples: Rejected form boundaries
        | field      | problem   |
        | last name  | too short |
        | first name | too long  |
        | last name  | too long  |
        | password   | too short |
        | password   | too long  |

      # The form does not currently enforce the backend's two-character minimum.
      @boundary @known-defect
      Examples: Names shorter than the minimum
        | field      | problem   |
        | first name | too short |
        | last name  | too short |

      # The form does not currently enforce the backend's 254-character email maximum.
      @boundary @known-defect
      Examples: Email beyond the maximum
        | field         | problem  |
        | email address | too long |

    @api @validation
    Scenario Outline: A visitor can register with a permitted name punctuation mark
      When a visitor registers with an accepted <field> value (<problem>)
      Then the visitor is registered

      @boundary
      Examples: Permitted punctuation in names
        | field      | problem       |
        | first name | with a hyphen |
        | first name | with an apostrophe |
        | last name  | with a hyphen |
        | last name  | with an apostrophe |

    @ui @validation
    Scenario Outline: A visitor can submit a permitted name punctuation mark
      Given a visitor is on the sign-up page
      When the visitor fills in the sign-up form with valid details
      And the visitor enters an accepted <field> value (<problem>)
      Then the visitor can submit the form

      @boundary
      Examples: Permitted form punctuation in names
        | field      | problem       |
        | first name | with a hyphen |
        | first name | with an apostrophe |
        | last name  | with a hyphen |
        | last name  | with an apostrophe |
