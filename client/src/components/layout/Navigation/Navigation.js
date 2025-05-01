import React, { useState, useEffect } from 'react';
import { Navbar, Nav, Button, Container, Dropdown } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import AuthModal from '../../auth/AuthModal/AuthModal';
import RegisterModal from '../../auth/RegisterModal/RegisterModal';

const Navigation = () => {
  const navigate = useNavigate();
  const [currentUser, setCurrentUser] = useState(null);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);

  useEffect(() => {
    const user = JSON.parse(localStorage.getItem('currentUser') || 'null');
    setCurrentUser(user);
  }, []);

  const handleLoginSuccess = (userData) => {
    setCurrentUser(userData);
    setShowAuthModal(false);
  };

  const handleLogout = () => {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('userToken');
    setCurrentUser(null);
    navigate('/');
    window.dispatchEvent(new Event('storage'));
  };

  return (
    <>
      <Navbar bg="orange" variant="dark" expand="lg" className="top-navbar">
        <Container>
          <Navbar.Brand as={Link} to="/">Language + Learning</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="ms-auto align-items-center">
              {currentUser ? (
                <>
                  <Navbar.Text className="me-3">
                    Привет, <strong>{currentUser.username}</strong> ({currentUser.role})
                  </Navbar.Text>

                  <Dropdown align="end" className="me-2">
                    <Dropdown.Toggle variant="outline-light" id="dropdown-menu">
                      Меню
                    </Dropdown.Toggle>
                    <Dropdown.Menu>
                      <Dropdown.Item as={Link} to="/profile">Профиль</Dropdown.Item>
                      {currentUser.role === 'Admin' && (
                        <>
                          <Dropdown.Item as={Link} to="/admin/users">
                            Админ-панель
                          </Dropdown.Item>
                          <Dropdown.Item onClick={() => setShowRegisterModal(true)}>
                            Зарегистрировать пользователя
                          </Dropdown.Item>
                        </>
                      )}
                    </Dropdown.Menu>
                  </Dropdown>

                  <Button variant="outline-light" onClick={handleLogout}>
                    Выход
                  </Button>
                </>
              ) : (
                <Button 
                  variant="outline-light" 
                  onClick={() => setShowAuthModal(true)}
                >
                  Вход
                </Button>
              )}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>

      <AuthModal
        show={showAuthModal}
        onHide={() => setShowAuthModal(false)}
        onLoginSuccess={handleLoginSuccess}
      />

      {currentUser?.role === 'Admin' && (
        <RegisterModal
          show={showRegisterModal}
          onHide={() => setShowRegisterModal(false)}
          currentUser={currentUser}
        />
      )}
    </>
  );
};

export default Navigation;