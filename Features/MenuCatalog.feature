Feature: Menu

  What a guest can do with the restaurant's menu: browse by category, sort, search,
  and open a dish — plus a mixed proof that a dish the kitchen adds also shows up on
  the storefront. The restaurant's menu is shared and changes over time, so scenarios
  that depend on what happens to be on it are tagged @data-dependent and check that
  "all the dishes shown" fit, rather than counting them.

  Background:
    Given the restaurant has a location

  Rule: Guests can browse the menu by category

    @api
    Scenario Outline: Browsing a category only shows dishes of that category
      When a guest browses <category> dishes
      Then the menu is shown
      And all the dishes shown are <category> dishes

      @smoke
      Examples: A category that is always stocked
        | category    |
        | main course |

      @data-dependent
      Examples: Categories the menu may not stock at the moment
        | category  |
        | appetizer |
        | dessert   |

    @ui @data-dependent
    Scenario Outline: Filtering the storefront by category shows only dishes of that category
      Given the kitchen has added a <category> dish
      When a guest filters the storefront menu by <category>
      Then every dish shown on the storefront belongs to <category>
      And the dishes added by this scenario are shown

      Examples: All categories
        | category    |
        | appetizer   |
        | main course |
        | dessert     |

  Rule: Guests can sort the menu

    @api
    Scenario Outline: Sorting by price puts the dishes in price order
      When a guest sorts the menu by <sorting>
      Then the menu is shown
      And the dishes are listed <order>

      Examples: Both directions
        | sorting              | order                           |
        | price, lowest first  | from cheapest to most expensive |
        | price, highest first | from most expensive to cheapest |

    @api @data-dependent
    Scenario Outline: Sorting by popularity puts dishes in popularity order
      Given the menu has dishes with a known popularity difference
      When a guest sorts the menu by <sorting>
      Then the fixture dishes are listed <popularity_order>

      Examples: Both popularity directions
        | sorting                         | popularity_order                   |
        | popularity, least popular first | from least popular to most popular |
        | popularity, most popular first  | from most popular to least popular |

    @ui @data-dependent
    Scenario Outline: Sorting the storefront by price puts rendered dishes in price order
      Given two main course dishes have been added
      When a guest sorts the storefront menu by <sorting>
      Then the storefront prices are listed <order>
      And the dishes added by this scenario are shown

      Examples: Both directions
        | sorting              | order                           |
        | price, lowest first  | from cheapest to most expensive |
        | price, highest first | from most expensive to cheapest |

    @ui @data-dependent
    Scenario Outline: Sorting the storefront by popularity orders this scenario's dishes
      Given the menu has dishes with a known popularity difference
      When a guest sorts the storefront menu by <sorting>
      Then the fixture dishes are listed <popularity_order> on the storefront

      Examples: Both directions
        | sorting                         | popularity_order                   |
        | popularity, least popular first | from least popular to most popular |
        | popularity, most popular first  | from most popular to least popular |

    @api @data-dependent
    Scenario: A category and a sort order can be combined
      When a guest browses main course dishes sorted by price, highest first
      Then all the dishes shown are main course dishes
      And the dishes are listed from most expensive to cheapest

  Rule: Guests are told when something does not exist

    @api @negative
    Scenario Outline: Opening something that does not exist
      When a guest opens a <item> that does not exist
      Then the guest is told that the <item> could not be found

      @dishes
      Examples: A dish
        | item |
        | dish |

      @locations
      Examples: A location
        | item     |
        | location |

  Rule: A dish the kitchen adds shows up for guests

    @api
    Scenario Outline: A guest can find a new dish by its name
      Given the kitchen has added a <category> dish
      When a guest searches the menu for the <name_match> of that dish's name
      Then that dish appears in the results

      Examples: Full and partial dish names
        | category    | name_match  |
        | main course | full name   |
        | dessert     | name prefix |

    @api
    Scenario Outline: A guest can open a new dish
      Given the kitchen has added a <category> dish
      When a guest opens that dish
      Then the dish is shown with:
        | detail | value       |
        | type   | <dish_type> |
        | status | Available   |

      Examples: All dish categories
        | category    | dish_type   |
        | appetizer   | Appetizer   |
        | main course | Main course |
        | dessert     | Dessert     |

    @ui
    Scenario Outline: A new dish can be opened from the storefront menu
      Given the kitchen has added a <category> dish
      When a guest opens that dish on the storefront
      Then that dish's details are shown on the storefront

      Examples: All dish categories
        | category    |
        | appetizer   |
        | main course |
        | dessert     |

    @ui
    Scenario: A new dish is visible on the storefront menu
      Given the kitchen has added a main course dish
      When a guest views the menu
      Then that dish is visible on the menu
